using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(12)]
    public class Migration_012 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;

            Create.Column("OrderTags").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("OrderStates").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("AutomationCommand").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("AutomationCommandValue").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("Hidden").OnTable("OrderTagGroups").AsBoolean().WithDefaultValue(false);
            Create.Column("SortOrder").OnTable("TicketTags").AsInt32().WithDefaultValue(0);
            Create.Column("CustomPrinterName").OnTable("Printers").AsString(128).Nullable();
            Create.Column("CustomPrinterData").OnTable("Printers").AsString().Nullable();

            Execute.Sql("Delete from AccountScreenValues where AccountScreen_Id is null");
            Create.Column("AccountScreenId").OnTable("AccountScreenValues").AsInt32().WithDefaultValue(0);
            Execute.Sql("Update AccountScreenValues set AccountScreenId = AccountScreen_Id");

            Delete.ForeignKey("FK_dbo.AccountScreenValues_dbo.AccountScreens_AccountScreen_Id").OnTable("AccountScreenValues");
            Delete.Index("IX_AccountScreen_Id").OnTable("AccountScreenValues");

            Delete.Column("AccountScreen_Id").FromTable("AccountScreenValues");

            Create.ForeignKey("FK_dbo.AccountScreenValues_dbo.AccountScreens_AccountScreenId")
                .FromTable("AccountScreenValues").ForeignColumn("AccountScreenId")
                .ToTable("AccountScreens").PrimaryColumn("Id");

            Create.Index("IX_AccountScreenId").OnTable("AccountScreenValues").OnColumn("AccountScreenId").Ascending();
        }

        public override void Down()
        {

        }
    }
}