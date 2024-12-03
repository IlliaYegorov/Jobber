using Dapper;
using Jobber.App.Entities;

namespace Jobber.App.DataAccess.Repositories;

public interface IJobProposalRepository
{
    public Task<bool> IsJobProposalExistsByUrl(string url);
    public Task AddJobProposalAsync(JobProposal jobProposal);
}

public class JobProposalRepository : IJobProposalRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public JobProposalRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> IsJobProposalExistsByUrl(string url)
    {
        const string query = "SELECT COUNT(1) FROM jobproposals WHERE url = @Url";
        using var connection = _dbConnectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(query, new { Url = url });
    }

    public async Task AddJobProposalAsync(JobProposal jobProposal)
    {
        const string query = @"
            INSERT INTO jobproposals (url, searchquery, paymenttype, createdatutc)
            VALUES (@Url, @SearchQuery, @PaymentType, @CreatedAtUtc)";

        var parameters = new
        {
            Url = jobProposal.Url.ToString(),
            jobProposal.SearchQuery,
            PaymentType = jobProposal.PaymentType.ToString(),
            jobProposal.CreatedAtUtc
        };

        using var connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(query, parameters);
    }
}
