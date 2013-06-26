using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(7)]
    public class Migration_007 : Migration
    {
        public override void Up()
        {
            Create.Column("Rounding").OnTable("TaxTemplates").AsInt32().WithDefaultValue(0);

        }

        public override void Down()
        {

        }
    }
}