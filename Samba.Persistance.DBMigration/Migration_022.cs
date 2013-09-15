using FluentMigrator;
using Samba.Localization.Properties;

namespace Samba.Persistance.DBMigration
{
    [Migration(22)]
    public class Migration_022 : Migration
    {
        public override void Up()
        {
            Create.Column("ShowOnEndOfDayReport").OnTable("States").AsBoolean().WithDefaultValue(false);
            Create.Column("ShowOnProductReport").OnTable("States").AsBoolean().WithDefaultValue(false);
            Create.Column("ShowOnTicket").OnTable("States").AsBoolean().WithDefaultValue(false);

            Execute.Sql("Insert into States (Name,GroupName,StateType,ShowOnEndOfDayReport,ShowOnProductReport,ShowOnTicket) values ('" + Resources.Gift + "','GStatus',2,1,1,1)");
            Execute.Sql("Insert into States (Name,GroupName,StateType,ShowOnEndOfDayReport,ShowOnProductReport,ShowOnTicket) values ('" + Resources.Status + "','Status',2,1,0,1)");

            Create.Column("SearchValueReplacePattern").OnTable("EntityScreens").AsString(256).Nullable();

        }

        public override void Down()
        {

        }
    }
}