using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Presentation.Services.Common.DataGeneration;

namespace Samba.Persistance.DBMigration
{
    [Migration(8)]
    public class Migration_008 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;

            Create.Column("HideZeroBalanceAccounts").OnTable("AccountScreenValues").AsBoolean().WithDefaultValue(false);
            Create.Column("DisplayAsTree").OnTable("AccountScreens").AsBoolean().WithDefaultValue(false);
            
            Create.Column("ReportPrinterId").OnTable("Terminals").AsInt32().WithDefaultValue(0);

            if (dc.Database.Connection.ConnectionString.EndsWith(".sdf"))
            {
                Delete.ForeignKey("FK_dbo.Terminals_dbo.Printers_ReportPrinter_Id").OnTable("Terminals");
                Delete.Index("IX_ReportPrinter_Id").OnTable("Terminals");
            }
            else
                Delete.ForeignKey("Terminal_ReportPrinter").OnTable("Terminals");

            Execute.Sql("Update Terminals set ReportPrinterId = ReportPrinter_Id");
            Delete.Column("ReportPrinter_Id").FromTable("Terminals");
            Create.Column("TransactionPrinterId").OnTable("Terminals").AsInt32().WithDefaultValue(0);
            Execute.Sql("Update Terminals set TransactionPrinterId = ReportPrinterId");
            Create.Column("DocumentTypeId").OnTable("AccountTransactionDocuments").AsInt32().WithDefaultValue(0);
            Create.Column("PrinterTemplateId").OnTable("AccountTransactionDocumentTypes").AsInt32().WithDefaultValue(0);



            if (dc != null)
            {
                if (dc.Set<PrinterTemplate>().Any(x => x.Name == Resources.CustomerReceiptTemplate)) return;

                var customerReceiptTemplate = new PrinterTemplate
                                                  {
                                                      Name = Resources.CustomerReceiptTemplate,
                                                      Template =
                                                          DataCreationService
                                                          .GetDefaultCustomerReceiptTemplate()
                                                  };
                dc.Set<PrinterTemplate>().Add(customerReceiptTemplate);
                dc.SaveChanges();

                var dt1Caption = string.Format(Resources.Customer_f, Resources.Cash);
                var dt2Caption = string.Format(Resources.Customer_f, Resources.CreditCard);

                Execute.Sql(string.Format("Update AccountTransactionDocumentTypes set PrinterTemplateId = {0} where Name = '{1}'", customerReceiptTemplate.Id, dt1Caption));
                Execute.Sql(string.Format("Update AccountTransactionDocumentTypes set PrinterTemplateId = {0} where Name = '{1}'", customerReceiptTemplate.Id, dt2Caption));
            }
        }

        public override void Down()
        {

        }
    }
}