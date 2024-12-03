using Hangfire.Server;

namespace Jobber.App.Hangfire;

public class HangfireJobExceptionFilter : IServerFilter
{
    private readonly ILogger<HangfireJobExceptionFilter> _logger;

    public HangfireJobExceptionFilter(ILogger<HangfireJobExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnPerforming(PerformingContext context)
    {
        _logger.LogInformation("Starting job: {JobId}", context.BackgroundJob.Id);
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Exception != null)
        {
            _logger.LogError(context.Exception, "An error occurred in job: {JobId}", context.BackgroundJob.Id);
        }
        else
        {
            _logger.LogInformation("Job completed successfully: {JobId}", context.BackgroundJob.Id);
        }
    }
}
