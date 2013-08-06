using System;
using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(18)]
    public class Migration_018 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;
            Create.Column("AutomationCommandMapData").OnTable("AccountScreens").AsString(int.MaxValue).Nullable();
            Create.Column("TicketLogs").OnTable("Tickets").AsString(int.MaxValue).Nullable();

            if (dc.Database.Connection.ConnectionString.EndsWith(".sdf"))
            {

            }
            else
            {
                var sql =
@"if exists(select * from sys.columns 
            where Name = N'DisplayAtPaymentScreen' and Object_ID = Object_ID(N'PaymentTypeMaps'))    
begin
   ALTER TABLE PaymentTypeMaps drop COLUMN DisplayAtPaymentScreen
end";

                var sql2 =
@"if exists(select * from sys.columns 
            where Name = N'DisplayUnderTicket' and Object_ID = Object_ID(N'PaymentTypeMaps'))    
begin
   ALTER TABLE PaymentTypeMaps drop COLUMN DisplayUnderTicket
end";

                Execute.Sql(sql);
                Execute.Sql(sql2);
            }

        }

        public override void Down()
        {

        }
    }
}