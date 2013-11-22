using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(24)]
    public class Migration_024 : Migration
    {
        public override void Up()
        {
            Create.Column("DisplayFormat").OnTable("EntityTypes").AsString(128).Nullable();
        }

        public override void Down()
        {

        }
    }
}