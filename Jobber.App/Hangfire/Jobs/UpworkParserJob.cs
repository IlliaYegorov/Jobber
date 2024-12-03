using Jobber.App.HttpClients;
using Jobber.App.Parsers;
using Jobber.App.Services;

namespace Jobber.App.Hangfire.Jobs;

public interface IUpworkParserJob 
{
    public Task ExecuteAsync(string searchQuery, string channelId);
}

public class UpworkParserJob : IUpworkParserJob
{
    private readonly ILogger<UpworkParserJob> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpworkParserJob(
        ILogger<UpworkParserJob> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ExecuteAsync(string searchQuery, string channelId)
    {
        await ProcessSearchQueryAsync(searchQuery, channelId);
    }

    private async Task ProcessSearchQueryAsync(string searchQuery, string channelId)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<IUpworkHttpClient>();
            var parser = scope.ServiceProvider.GetRequiredService<IUpworkJobProposalsParser>();
            var jobProposalService = scope.ServiceProvider.GetRequiredService<IJobProposalService>();
            var slackService = scope.ServiceProvider.GetRequiredService<ISlackService>();

            var html = await httpClient.GetHtmlAsync(searchQuery);

            parser.LoadHtml(html);
            var jobProposals = parser.Parse();

            foreach (var jobProposal in jobProposals)
            {
                var isExists = await jobProposalService.IsJobProposalExistsAsync(jobProposal.Url.ToString());
                if (isExists)
                {
                    continue;
                }

                jobProposal.SearchQuery = searchQuery;

                await jobProposalService.AddJobProposalAsync(jobProposal);
                await slackService.SendProposalToSlackAsync(jobProposal, channelId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing search query {searchQuery}");
            throw;
        }
    }
}
