using System;
using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(20)]
    public class Migration_020 : Migration
    {
        public override void Up()
        {
            Create.Column("DisplayUnderTicket2").OnTable("AutomationCommandMaps").AsBoolean().WithDefaultValue(false);
            Create.Column("DisplayOnCommandSelector").OnTable("AutomationCommandMaps").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {

        }
    }
}