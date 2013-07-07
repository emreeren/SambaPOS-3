using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;

namespace Samba.Persistance.DBMigration
{
    [Migration(10)]
    public class Migration_010 : Migration
    {
        public override void Up()
        {
            Create.Column("DisplayOnTicketList").OnTable("AutomationCommandMaps").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {

        }
    }
}