using Npgsql;
using System.Data.Common;

namespace Jobber.App.DataAccess;

public interface IDbConnectionFactory
{
    public DbConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public DbConnection CreateConnection() =>
        new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
}
