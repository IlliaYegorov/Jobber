using FluentMigrator.Runner;
using Hangfire;
using Hangfire.PostgreSql;
using Jobber.App.DataAccess;
using Jobber.App.DataAccess.Migrations;
using Jobber.App.DataAccess.Repositories;
using Jobber.App.Hangfire;
using Jobber.App.Hangfire.Jobs;
using Jobber.App.HttpClients;
using Jobber.App.Parsers;
using Jobber.App.Services;
using Jobber.App.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace Jobber.App;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddLog4Net("App_Data/log4net.config");
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        builder.Services.AddScoped<IJobProposalRepository, JobProposalRepository>();
        builder.Services.AddScoped<IJobProposalService, JobProposalService>();
        builder.Services.AddScoped<ISlackService, SlackService>();
        builder.Services.AddScoped<IUpworkJobProposalsParser, UpworkJobProposalsParser>();
        builder.Services.AddSingleton<IUpworkParserJob, UpworkParserJob>();
        builder.Services.Configure<SearchQueriesSettings>(builder.Configuration.GetSection("SearchQueries"));
        builder.Services.Configure<SlackSettings>(builder.Configuration.GetSection("Slack"));

        builder.Services.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
                .ScanIn(typeof(AddJobProposalTable).Assembly).For.Migrations());

        builder.Services.AddHangfire(config =>
        {
            config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddHangfireServer(x => x.WorkerCount = Environment.ProcessorCount * 5);

        builder.Services.AddControllers();

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Jobber.App.App_Data.upworksettings.json";

        using var stream = assembly.GetManifestResourceStream(resourceName) ??
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var upworkSettings = JsonSerializer.Deserialize<UpworkSettings>(json)
                            ?? throw new InvalidOperationException("Failed to deserialize Upwork settings.");
        builder.Services.AddSingleton(x => upworkSettings);

        builder.Services.AddHttpClient<IUpworkHttpClient, UpworkHttpClient>(client =>
        {
            client.BaseAddress = new Uri(upworkSettings.BaseUrl + "/nx/search/jobs/");

            foreach (var header in upworkSettings.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        app.MapControllers();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions()
        {
            DarkModeEnabled = true,
            DashboardTitle = "Pantera Dashboard"
        });

        GlobalJobFilters.Filters.Add(new HangfireJobExceptionFilter(app.Services.GetRequiredService<ILogger<HangfireJobExceptionFilter>>()));

        var searchQueriesSettings = app.Services.GetRequiredService<IOptions<SearchQueriesSettings>>().Value;
        var slackSettings = app.Services.GetRequiredService<IOptions<SlackSettings>>().Value;

        RecurringJob.AddOrUpdate<IUpworkParserJob>(
            "UpworkParserJob-FixedPrice-Azure",
            job => job.ExecuteAsync(searchQueriesSettings.FixedPrice["Azure"], slackSettings.Channels.FixedPrice["Azure"]),
            Cron.MinuteInterval(5));

        RecurringJob.AddOrUpdate<IUpworkParserJob>(
            "UpworkParserJob-FixedPrice-CSharp",
            job => job.ExecuteAsync(searchQueriesSettings.FixedPrice["CSharp"], slackSettings.Channels.FixedPrice["CSharp"]),
            Cron.MinuteInterval(5));

        RecurringJob.AddOrUpdate<IUpworkParserJob>(
            "UpworkParserJob-FixedPrice-Frontend",
            job => job.ExecuteAsync(searchQueriesSettings.FixedPrice["Frontend"], slackSettings.Channels.FixedPrice["Frontend"]),
            Cron.MinuteInterval(5));

        RecurringJob.AddOrUpdate<IUpworkParserJob>(
            "UpworkParserJob-FixedPrice-Swift",
            job => job.ExecuteAsync(searchQueriesSettings.FixedPrice["Swift"], slackSettings.Channels.FixedPrice["Swift"]),
            Cron.MinuteInterval(5));

        RecurringJob.AddOrUpdate<IUpworkParserJob>(
            "UpworkParserJob-Hourly-CSharpSenior",
            job => job.ExecuteAsync(searchQueriesSettings.Hourly["CSharpSenior"], slackSettings.Channels.Hourly["CSharpSenior"]),
            Cron.MinuteInterval(5));

        RecurringJob.AddOrUpdate<IUpworkParserJob>(
            "UpworkParserJob-Hourly-CSharpMiddle",
            job => job.ExecuteAsync(searchQueriesSettings.Hourly["CSharpMiddle"], slackSettings.Channels.Hourly["CSharpMiddle"]),
            Cron.MinuteInterval(5));

        app.Run();
    }
}
