using Jobber.App.Enums;
using System.Text;

namespace Jobber.App.Entities;

public class JobProposal
{
    public int Id { get; set; }
    public Uri Url { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Price { get; set; }
    public string SearchQuery { get; set; }
    public string Duration { get; set; }
    public IEnumerable<string> Skills { get; set; }
    public PaymentType PaymentType { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public string ToSlackMessage()
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!channel>");
        sb.AppendLine($":sparkles: *Title:* *<{Url}|{Title}>*");
        sb.AppendLine();

        sb.AppendLine(":memo: *Description:*");
        if (!string.IsNullOrWhiteSpace(Description))
        {
            foreach (var line in Description.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                sb.AppendLine($"      {line}");
            }
        }
        else
        {
            sb.AppendLine("      No description provided.");
        }

        sb.AppendLine();
        sb.AppendLine($":hourglass_flowing_sand: *Duration:* {Duration}");
        sb.AppendLine($":hammer_and_wrench: *Skills:* {(Skills != null && Skills.Any() ? string.Join(", ", Skills) : "None")}");
        sb.AppendLine($":money_with_wings: *Lead Payment Type:* {PaymentType}");
        sb.AppendLine($":dollar: *Payment Amount:* {Price}");
        sb.AppendLine();
        sb.AppendLine("*--------------------------------------------------------*");

        return sb.ToString();
    }
}
