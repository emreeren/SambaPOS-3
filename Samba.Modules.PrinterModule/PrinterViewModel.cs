using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FluentValidation;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.PrinterModule
{
    class PrinterValidator : EntityValidator<Printer>
    {
        public PrinterValidator()
        {
            RuleFor(x => x.CharsPerLine).GreaterThan(0);
            RuleFor(x => x.CodePage).GreaterThan(0);
        }
    }

    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class PrinterViewModel : EntityViewModelBase<Printer>
    {
        private readonly IPrinterService _printerService;

        [ImportingConstructor]
        public PrinterViewModel(IPrinterService printerService)
        {
            _printerService = printerService;
        }

        public IList<string> PrinterTypes { get { return new[] { Resources.TicketPrinter, Resources.Text, Resources.Html, Resources.PortPrinter, Resources.DemoPrinter }; } }

        public string ShareName { get { return Model.ShareName; } set { Model.ShareName = value; } }
        public string PrinterType
        {
            get { return PrinterTypes[Model.PrinterType]; }
            set { Model.PrinterType = PrinterTypes.IndexOf(value); }
        }

        public int CodePage { get { return Model.CodePage; } set { Model.CodePage = value; } }
        public int CharsPerLine { get { return Model.CharsPerLine; } set { Model.CharsPerLine = value; } }
        public int PageHeight { get { return Model.PageHeight; } set { Model.PageHeight = value; } }

        private IEnumerable<string> _printerNames;
        public IEnumerable<string> PrinterNames
        {
            get { return _printerNames ?? (_printerNames = GetPrinterNames()); }
        }

        private IEnumerable<string> GetPrinterNames()
        {
            return _printerService.GetPrinterNames();
        }

        public override Type GetViewType()
        {
            return typeof(PrinterView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Printer;
        }

        protected override AbstractValidator<Printer> GetValidator()
        {
            return new PrinterValidator();
        }
    }
}
