using System;
using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;

namespace Samba.Persistance.DBMigration
{
    [Migration(3)]
    public class Migration_003 : Migration
    {
        public override void Up()
        {
            Create.Column("AccountTypeId").OnTable("TicketEntities").AsInt32().WithDefaultValue(0);
            Create.Column("AskTicketType").OnTable("EntityScreens").AsBoolean().WithDefaultValue(false);
            Create.Column("SortOrder").OnTable("TicketTypes").AsInt32().WithDefaultValue(0);
            Delete.Column("AccountId").FromTable("Tickets");
            Delete.Column("AccountName").FromTable("Tickets");
            Delete.Column("AccountTypeId").FromTable("Tickets");
        }

        public override void Down()
        {

        }
    }
}