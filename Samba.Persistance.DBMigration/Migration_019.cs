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
            Create.Column("CategoryColumnCount").OnTable("ScreenMenus").AsInt32().WithDefaultValue(0);
            Create.Column("CategoryColumnWidthRate").OnTable("ScreenMenus").AsDecimal().WithDefaultValue(0);
            Execute.Sql("Update ScreenMenus set CategoryColumnWidthRate=25");
            Execute.Sql("Update ScreenMenus set CategoryColumnCount=1");
        }

        public override void Down()
        {

        }
    }
}