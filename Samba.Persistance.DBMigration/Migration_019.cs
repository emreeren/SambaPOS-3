using System;
using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(19)]
    public class Migration_019 : Migration
    {
        public override void Up()
        {
            Create.Column("ScreenMenuId").OnTable("Departments").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {

        }
    }
}