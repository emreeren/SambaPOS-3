using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(21)]
    public class Migration_021 : Migration
    {
        public override void Up()
        {
            Create.Table("TaskCustomFields")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(128).Nullable()
                .WithColumn("TaskTypeId").AsInt32().WithDefaultValue(0)
                .WithColumn("FieldType").AsInt32().WithDefaultValue(0)
                .WithColumn("EditingFormat").AsString(128).Nullable()
                .WithColumn("DisplayFormat").AsString(128).Nullable();


            Create.Column("CustomData").OnTable("Tasks").AsString(int.MaxValue).Nullable();

            Create.ForeignKey("FK_dbo.TaskCustomFields_dbo.TaskTypes_TaskTypeId")
                  .FromTable("TaskCustomFields").ForeignColumn("TaskTypeId")
                  .ToTable("TaskTypes").PrimaryColumn("Id");

            Create.Index("IX_TaskTypeId").OnTable("TaskCustomFields").OnColumn("TaskTypeId").Ascending();


            Delete.Table("TaskTypeEntityTypes");

            Execute.Sql(@"
UPDATE AppActions 
SET Parameter = CAST(REPLACE(CAST(Parameter as NVarchar(4000)),'""Key"":""Value""','""Key"":""FieldValue""') AS NText)
WHERE ActionType = 'UpdateEntityData'");

            Execute.Sql(@"
Update ActionContainers 
set ParameterValues = CAST(REPLACE(CAST(ParameterValues as NVarchar(4000)),'[:Value]','[:CommandValue]') AS NText)
where AppRuleId in (select Id from AppRules where EventName = 'AutomationCommandExecuted')");


            Execute.Sql(@"
Update ActionContainers 
set ParameterValues = CAST(REPLACE(CAST(ParameterValues as NVarchar(4000)),'[:Value]','[:LoopValue]') AS NText)
where AppRuleId in (select Id from AppRules where EventName = 'ValueLooped')");
            
            Execute.Sql(@"
Update ActionContainers 
set ParameterValues = CAST(REPLACE(CAST(ParameterValues as NVarchar(4000)),'[:Value]','[:NumberpadValue]') AS NText)
where AppRuleId in (select Id from AppRules where EventName = 'NumberpadValueEntered')");
        }

        public override void Down()
        {

        }
    }
}