using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class TerminalViewModel : EntityViewModelBase<Terminal>
    {
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public TerminalViewModel(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public bool IsDefault { get { return Model.IsDefault; } set { Model.IsDefault = value; } }
        public bool AutoLogout { get { return Model.AutoLogout; } set { Model.AutoLogout = value; } }
        public int? ReportPrinterId { get { return Model.ReportPrinterId; } set { Model.ReportPrinterId = value.GetValueOrDefault(0); } }
        public int? TransactionPrinterId { get { return Model.TransactionPrinterId; } set { Model.TransactionPrinterId = value.GetValueOrDefault(0); } }

        public IEnumerable<Printer> Printers { get; private set; }
        public IEnumerable<PrinterTemplate> PrinterTemplates { get; private set; }

        public override Type GetViewType()
        {
            return typeof(TerminalView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Terminal;
        }

        protected override void Initialize()
        {
            Printers = Workspace.All<Printer>();
            PrinterTemplates = Workspace.All<PrinterTemplate>();
        }

        protected override string GetSaveErrorMessage()
        {
            if (Model.IsDefault)
            {
                var terminal = _settingService.GetDefaultTerminal();
                if (terminal != null && terminal.Id != Model.Id)
                    return Resources.SaveErrorMultipleDefaultTerminals;
            }
            return base.GetSaveErrorMessage();
        }
    }
}
