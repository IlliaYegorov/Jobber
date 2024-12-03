using Jobber.App.Entities;
using SlackAPI;

namespace Jobber.App.Services;

public interface ISlackService
{
    public Task SendProposalToSlackAsync(JobProposal proposal, string channelId);
}

public class SlackService : ISlackService
{
    private readonly ILogger<SlackService> _logger;
    private readonly string _slackToken;

    public SlackService(
        ILogger<SlackService> logger, 
        IConfiguration configuration)
    {
        _logger = logger;
        _slackToken = configuration.GetSection("Slack:OAuthToken").Get<string>();
    }

    public async Task SendProposalToSlackAsync(JobProposal proposal, string channelId)
    {
        try
        {
            var slackClient = new SlackTaskClient(_slackToken);
            var message = proposal.ToSlackMessage();
            var response = await slackClient.PostMessageAsync(channelId, message);

            if (!response.ok)
            {
                _logger.LogError("Failed to send proposal to Slack. Error: {Error}", response.error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending proposal to Slack");
        }
    }
}
