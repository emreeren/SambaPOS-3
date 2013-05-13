using System;
using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;

namespace Samba.Persistance.DBMigration
{
    [Migration(4)]
    public class Migration_004 : Migration
    {
        public override void Up()
        {
            Create.Column("Added").OnTable("PeriodicConsumptionItems").AsDecimal().WithDefaultValue(0);
            Create.Column("Removed").OnTable("PeriodicConsumptionItems").AsDecimal().WithDefaultValue(0);
            Execute.Sql("Update PeriodicConsumptionItems set Added = Purchase");
            Delete.Column("Purchase").FromTable("PeriodicConsumptionItems");
        }

        public override void Down()
        {

        }
    }
}