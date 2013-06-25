using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(6)]
    public class Migration_006 : Migration
    {
        public override void Up()
        {
            Create.Column("StartTime").OnTable("ProductTimers").AsInt32().WithDefaultValue(0);
            Execute.Sql("Update PrintJobs set WhatToPrint = 0 where WhatToPrint in (1,2,3,4)");
            Execute.Sql("Update PrintJobs set WhatToPrint = 1 where WhatToPrint = 5");
            Execute.Sql("Update PrintJobs set WhatToPrint = 2 where WhatToPrint = 6");
        }

        public override void Down()
        {

        }
    }
}