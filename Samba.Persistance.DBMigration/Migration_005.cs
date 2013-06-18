using System;
using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;

namespace Samba.Persistance.DBMigration
{
    [Migration(5)]
    public class Migration_005 : Migration
    {
        public override void Up()
        {
            Create.Column("UserId").OnTable("Payments").AsInt32().WithDefaultValue(0);
            Create.Column("UserId").OnTable("ChangePayments").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {

        }
    }
}