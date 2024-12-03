using HtmlAgilityPack;
using Jobber.App.Entities;
using Jobber.App.Enums;
using Jobber.App.Settings;

namespace Jobber.App.Parsers;

public interface IUpworkJobProposalsParser : IHtmlParser<JobProposal> { }

public class UpworkJobProposalsParser : BaseHtmlParser<JobProposal>, IUpworkJobProposalsParser
{
    private readonly string BASE_URL;
    private const string E2E_TEST_ATTRIBUTE = "data-test";
    private const string JOB_TITLE_LINK_ATTRIBUTE_VALUE = "job-tile-title-link";
    private const string JOB_DESCRIPTION_CLASS_VALUE = "text-body-sm";
    private const string JOB_INFO_CLASS_VALUE = "job-tile-info-list";

    public UpworkJobProposalsParser(UpworkSettings upworkSettings)
    {
        BASE_URL = upworkSettings.BaseUrl;
    }

    public override IEnumerable<JobProposal> Parse()
    {
        var parsedJobProposals = new List<JobProposal>();
        var jobProposalsHtml = GetElementsByTag("article").ToList();

        foreach (var jobProposalHtml in jobProposalsHtml)
        {
            if (jobProposalHtml == null)
            {
                continue;
            }

            try
            {
                LoadHtml(jobProposalHtml.InnerHtml);
            }
            catch (Exception)
            {
                continue;
            }

            var (url, text) = GetHyperlinksWithTextByAttribute(E2E_TEST_ATTRIBUTE, JOB_TITLE_LINK_ATTRIBUTE_VALUE)
                .FirstOrDefault();

            var jobTitle = text;
            var jobLink = AddBaseUrlIfNeeded(url);
            var jobDescription = GetJobDescriptionContent();
            var jobInfo = ExtractJobInfoFromElement(GetJobInfoElement());

            if (jobInfo == null)
            {
                continue;
            }

            if (
                jobDescription.ToLower().Contains("only freelances located in the US may apply") ||
                jobDescription.ToLower().Contains("united states only") ||
                jobDescription.ToLower().Contains("no agencies"))
            {
                continue;
            }

            if (
                jobTitle.ToLower().Contains("only freelances located in the US may apply") ||
                jobTitle.ToLower().Contains("united states only") ||
                jobTitle.ToLower().Contains("no agencies"))
            {
                continue;
            }

            var skills = GetSkills();
            var (priceAndType, _, duration) = jobInfo.Value;

            var paymentType = priceAndType.Split(": ").First() == "Hourly" ? PaymentType.Hourly : PaymentType.Fixed;
            var price = paymentType == PaymentType.Hourly ? priceAndType.Split(": ").Last().Replace("$", "") : duration.Replace("Est. budget: ", "").Replace("$", "");

            var parsedJobProposal = new JobProposal()
            {
                Title = jobTitle,
                Description = jobDescription,
                Skills = skills,
                Price = price,
                Duration = duration,
                Url = new Uri(jobLink),
                PaymentType = paymentType,
                CreatedAtUtc = DateTime.UtcNow
            };

            parsedJobProposals.Add(parsedJobProposal);
        }

        return parsedJobProposals;
    }

    private HtmlNode GetJobInfoElement()
    {
        return HtmlDocument.DocumentNode.SelectSingleNode($"//ul[contains(@class, '{JOB_INFO_CLASS_VALUE}')]");
    }

    private static (string PriceAndType, string ExperienceLevel, string Duration)? ExtractJobInfoFromElement(HtmlNode jobInfoElement)
    {
        if (jobInfoElement == null) return null;

        var listItems = jobInfoElement.Descendants("li").ToList();
        if (listItems.Count < 3) return null;

        var priceAndType = listItems[0].InnerText.Trim();
        var experienceLevel = listItems[1].InnerText.Trim();
        var duration = listItems[2].InnerText.Trim();

        return (priceAndType, experienceLevel, duration);
    }

    private string GetJobDescriptionContent()
    {
        var descriptionNode = HtmlDocument.DocumentNode.SelectSingleNode($"//p[contains(@class, '{JOB_DESCRIPTION_CLASS_VALUE}')]");
        return descriptionNode?.InnerText.Trim() ?? string.Empty;
    }

    private IEnumerable<string> GetSkills()
    {
        return HtmlDocument.DocumentNode
            .SelectNodes("//button[@class='air3-token']")
            ?.Select(node => node.InnerText.Trim())
            .ToList() ?? [];
    }

    private string AddBaseUrlIfNeeded(string partialUrl)
    {
        if (Uri.IsWellFormedUriString(partialUrl, UriKind.Absolute))
            return partialUrl;

        return $"{BASE_URL}{partialUrl}";
    }
}
