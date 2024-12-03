using Jobber.App.DataAccess.Repositories;
using Jobber.App.Entities;

namespace Jobber.App.Services;

public interface IJobProposalService
{
    public Task<bool> IsJobProposalExistsAsync(string url);
    public Task AddJobProposalAsync(JobProposal jobProposal);
}

public class JobProposalService : IJobProposalService
{
    private readonly IJobProposalRepository _jobProposalRepository;
    private readonly ILogger<JobProposalService> _logger;

    public JobProposalService(IJobProposalRepository jobProposalRepository, ILogger<JobProposalService> logger)
    {
        _jobProposalRepository = jobProposalRepository;
        _logger = logger;
    }

    public async Task<bool> IsJobProposalExistsAsync(string url)
    {
        try
        {
            _logger.LogInformation("Checking if job proposal exists for URL: {Url}", url);
            var exists = await _jobProposalRepository.IsJobProposalExistsByUrl(url);
            _logger.LogInformation("Job proposal existence check complete for URL: {Url}, Exists: {Exists}", url, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if job proposal exists for URL: {Url}", url);
            throw;
        }
    }

    public async Task AddJobProposalAsync(JobProposal jobProposal)
    {
        try
        {
            _logger.LogInformation("Attempting to add a new job proposal with ID: {Id} and URL: {Url}", jobProposal.Id, jobProposal.Url);
            await _jobProposalRepository.AddJobProposalAsync(jobProposal);
            _logger.LogInformation("Successfully added job proposal with ID: {Id} and URL: {Url}", jobProposal.Id, jobProposal.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding job proposal with ID: {Id} and URL: {Url}", jobProposal.Id, jobProposal.Url);
            throw;
        }
    }
}
