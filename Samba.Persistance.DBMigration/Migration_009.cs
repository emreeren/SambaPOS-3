using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;

namespace Samba.Persistance.DBMigration
{
    [Migration(9)]
    public class Migration_009 : Migration
    {
        public override void Up()
        {
            Create.Column("PrimaryFieldName").OnTable("EntityTypes").AsString(128).Nullable();
            Create.Column("PrimaryFieldFormat").OnTable("EntityTypes").AsString(128).Nullable();
        }

        public override void Down()
        {

        }
    }
}