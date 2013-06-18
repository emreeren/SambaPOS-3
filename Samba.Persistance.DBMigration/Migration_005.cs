using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(5)]
    public class Migration_005 : Migration
    {
        public override void Up()
        {
            Create.Column("UserId").OnTable("Payments").AsInt32().WithDefaultValue(0);
            Create.Column("UserId").OnTable("ChangePayments").AsInt32().WithDefaultValue(0);
            Create.Column("FontSize").OnTable("PaymentTypes").AsInt32().WithDefaultValue(0);
            Create.Column("FontSize").OnTable("CalculationSelectors").AsInt32().WithDefaultValue(0);
            Create.Column("FontSize").OnTable("AutomationCommands").AsInt32().WithDefaultValue(0);

            Execute.Sql("Update PaymentTypes set FontSize = 40");
            Execute.Sql("Update CalculationSelectors set FontSize = 30");
            Execute.Sql("Update AutomationCommands set FontSize = 30");
        }

        public override void Down()
        {

        }
    }
}