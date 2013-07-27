using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(13)]
    public class Migration_013 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;
            Create.Column("ToggleCalculation").OnTable("CalculationTypes").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {

        }
    }
}