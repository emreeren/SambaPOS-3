using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using FluentValidation;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
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
            EditProcessorSettingsCommand = new CaptionCommand<string>(Resources.Settings, OnEditProcessorSettings);
        }

        public ICaptionCommand EditProcessorSettingsCommand { get; set; }

        public IList<string> PrinterTypes { get { return new[] { Resources.TicketPrinter, Resources.Text, Resources.Html, Resources.PortPrinter, Resources.DemoPrinter, Resources.WindowsPrinter }; } }

        public string ShareName
        {
            get { return Model.ShareName; }
            set
            {
                Model.ShareName = value;
                RaisePropertyChanged(() => IsProcessorSelected);
            }
        }

        public string PrinterType
        {
            get { return PrinterTypes[Model.PrinterType]; }
            set
            {
                Model.PrinterType = PrinterTypes.IndexOf(value);
                Description = GetPrinterTypeDescription(Model.PrinterType);
            }
        }

        public int CodePage { get { return Model.CodePage; } set { Model.CodePage = value; } }
        public int CharsPerLine { get { return Model.CharsPerLine; } set { Model.CharsPerLine = value; } }
        public int PageHeight { get { return Model.PageHeight; } set { Model.PageHeight = value; } }
        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged(() => Description); }
        }

        public bool IsProcessorSelected { get { return _printerService.GetPrinterProcessor(ShareName) != null; } }

        private IEnumerable<string> _printerNames;
        private string _description;

        public IEnumerable<string> PrinterNames
        {
            get { return _printerNames ?? (_printerNames = GetPrinterNames()); }
        }

        private IEnumerable<string> GetPrinterNames()
        {
            var result = new List<string>();
            result.AddRange(_printerService.GetPrinterNames());
            result.AddRange(_printerService.GetProcessorNames());
            return result;
        }

        private void OnEditProcessorSettings(string obj)
        {
            var processor = _printerService.GetPrinterProcessor(ShareName);
            if (processor != null) processor.EditSettings();
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

        private string GetPrinterTypeDescription(int printerType)
        {
            switch (printerType)
            {
                case 0: return "Ticket Printer: Sends output to a ESC/POS Emulated thermal ticket printer. It won't work with other printer types such as inkjet or laser printers";
                case 1: return "Text Printer: Use for text only formatting. Useful for dot matrix printers.";
                case 2: return "HTML Printer: Uses HTML tags for formatting document. Also useful for printing tickets with an inkjet or laser printer.";
                case 3: return "Port Printer: Sends output directly to a com port. Useful for controlling devices such as customer displays or cash drawers.";
                case 4: return "Demo Printer: Enter a fake printer name and open Notepad application. Printout will directly appear in notepad so you can test your templates without wasting paper. If you enter a File Name it will save output in that file.";
                case 5: return "Windows Printer: Choose a printer to print with driver page settings. If there is no printer matches with selected printer name windows print dialog displays. You can enter a fake printer name for forcing print dialog display.";
                default: return "";
            }
        }
    }
}
