using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.PrinterModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class PrinterTemplateViewModel : EntityViewModelBase<PrinterTemplate>
    {
        private readonly IPrinterService _printerService;

        [ImportingConstructor]
        public PrinterTemplateViewModel(IPrinterService printerService)
        {
            _printerService = printerService;
        }

        public string HeaderTemplate { get { return Model.HeaderTemplate; } set { Model.HeaderTemplate = value; } }
        public string LineTemplate { get { return Model.LineTemplate; } set { Model.LineTemplate = value; } }
        public string FooterTemplate { get { return Model.FooterTemplate; } set { Model.FooterTemplate = value; } }
        public bool MergeLines { get { return Model.MergeLines; } set { Model.MergeLines = value; } }

        public override Type GetViewType()
        {
            return typeof(PrinterTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.PrinterTemplate;
        }

        private IDictionary<string, string> _descriptions;
        public IDictionary<string, string> Descriptions
        {
            get { return _descriptions ?? (_descriptions = CreateDescriptions()); }
        }

        private IDictionary<string, string> CreateDescriptions()
        {
            return _printerService.CreateTagDescriptions();
        }
    }
}
