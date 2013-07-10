using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(11)]
    public class Migration_011 : Migration
    {
        public override void Up()
        {
            Create.Column("SortOrder").OnTable("AccountScreens").AsInt32().WithDefaultValue(0);
            Create.Column("LastModifiedUserName").OnTable("Tickets").AsString(128).Nullable();
            Create.Column("AccountBalanceDisplayFormat").OnTable("EntityTypes").AsString(128).Nullable();
        }

        public override void Down()
        {

        }
    }
}