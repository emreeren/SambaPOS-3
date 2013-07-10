using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(11)]
    public class Migration_011 : Migration
    {
        public override void Up()
        {
            Create.Column("SortOrder").OnTable("AccountScreens").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {

        }
    }
}