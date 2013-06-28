using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Persistance.Implementations
{
    [Export(typeof(IPrinterDao))]
    class PrinterDao : IPrinterDao
    {
        [ImportingConstructor]
        public PrinterDao()
        {
            ValidatorRegistry.RegisterDeleteValidator(new PrinterDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator<PrinterTemplate>(x => Dao.Exists<PrinterMap>(y => y.PrinterTemplateId == x.Id), Resources.PrinterTemplate, Resources.PrintJob);
        }

        public IEnumerable<Printer> GetPrinters()
        {
            return Dao.Query<Printer>();
        }

        public IEnumerable<PrinterTemplate> GetPrinterTemplates()
        {
            return Dao.Query<PrinterTemplate>();
        }
    }

    public class PrinterDeleteValidator : SpecificationValidator<Printer>
    {
        public override string GetErrorMessage(Printer model)
        {
            if (Dao.Exists<Terminal>(x => x.ReportPrinterId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Printer, Resources.Terminal);        
            if (Dao.Exists<Terminal>(x => x.TransactionPrinterId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Printer, Resources.Terminal);
            if (Dao.Exists<PrinterMap>(x => x.PrinterId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Printer, Resources.PrintJob);
            return "";
        }
    }
}
