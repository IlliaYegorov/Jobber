using FluentMigrator;

namespace Jobber.App.DataAccess.Migrations;

[Migration(20241202)]
public class AddJobProposalTable : Migration
{
    public override void Up()
    {
        Create.Table("jobproposals")
            .WithColumn("id").AsInt32().Identity().PrimaryKey()
            .WithColumn("url").AsString(1000).NotNullable().Unique()
            .WithColumn("searchquery").AsString(500).NotNullable()
            .WithColumn("paymenttype").AsString(10).NotNullable()
            .WithColumn("createdatutc").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("ix_jobproposals_url")
            .OnTable("jobproposals")
            .OnColumn("url")
            .Unique()
            .WithOptions().NonClustered();
    }

    public override void Down()
    {
        Delete.Index("ix_jobproposals_url").OnTable("jobproposals");
        Delete.Table("jobproposals");
    }
}
