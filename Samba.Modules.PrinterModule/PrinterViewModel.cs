using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FluentValidation;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.PrinterModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class PrinterViewModel : EntityViewModelBase<Printer>
    {
        private readonly IPrinterService _printerService;
        private readonly IUserInteraction _userInteraction;

        [ImportingConstructor]
        public PrinterViewModel(IPrinterService printerService, IUserInteraction userInteraction)
        {
            _printerService = printerService;
            _userInteraction = userInteraction;
            EditCustomPrinterSettingsCommand = new CaptionCommand<string>(Resources.Settings,
                                                                          OnEditCustomPrinterSettings, CanEditCustomPrinterSettings);
        }

        public ICaptionCommand EditCustomPrinterSettingsCommand { get; set; }

        public IList<string> PrinterTypes { get { return new[] { Resources.TicketPrinter, Resources.Text, Resources.Html, Resources.PortPrinter, Resources.DemoPrinter, Resources.WindowsPrinter, Resources.CustomPrinter ,Resources.RawPrinter}; } }

        public string ShareName
        {
            get { return Model.ShareName; }
            set { Model.ShareName = value; }
        }

        public string PrinterType
        {
            get { return PrinterTypes[Model.PrinterType]; }
            set
            {
                Model.PrinterType = PrinterTypes.IndexOf(value);
                Description = GetPrinterTypeDescription(Model.PrinterType);
                if (!IsCustomPrinter)
                {
                    CustomPrinterName = "";
                    CustomPrinterData = "";
                }
                RaisePropertyChanged(() => IsCustomPrinter);
            }
        }

        public int CodePage { get { return Model.CodePage; } set { Model.CodePage = value; } }
        public int CharsPerLine { get { return Model.CharsPerLine; } set { Model.CharsPerLine = value; } }
        public int PageHeight { get { return Model.PageHeight; } set { Model.PageHeight = value; } }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged(() => Description); }
        }

        public string CustomPrinterName { get { return Model.CustomPrinterName; } set { Model.CustomPrinterName = value; } }
        public string CustomPrinterData { get { return Model.CustomPrinterData; } set { Model.CustomPrinterData = value; } }

        public bool IsCustomPrinter { get { return Model.IsCustomPrinter; } }

        public IEnumerable<string> CustomPrinterNames { get { return _printerService.GetCustomPrinterNames(); } }

        private IEnumerable<string> _printerNames;
        public IEnumerable<string> PrinterNames
        {
            get { return _printerNames ?? (_printerNames = GetPrinterNames()); }
        }

        private IEnumerable<string> GetPrinterNames()
        {
            var result = new List<string>();
            result.AddRange(_printerService.GetPrinterNames());
            return result;
        }

        private bool CanEditCustomPrinterSettings(string arg)
        {
            return !string.IsNullOrEmpty(CustomPrinterName);
        }

        private void OnEditCustomPrinterSettings(string obj)
        {
            var settingsObject = _printerService.GetCustomPrinterData(CustomPrinterName, CustomPrinterData);
            _userInteraction.EditProperties(settingsObject);
            Model.UpdateCustomSettings(settingsObject);
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
    
    class PrinterValidator : EntityValidator<Printer>
    {
        public PrinterValidator()
        {
            RuleFor(x => x.CharsPerLine).GreaterThan(0);
            RuleFor(x => x.CodePage).GreaterThan(0);
        }
    }
}
