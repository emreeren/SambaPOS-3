using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using ICSharpCode.AvalonEdit.Document;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
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

        public string Template { get { return Model.Template; } set { Model.Template = value; } }
        public bool MergeLines { get { return Model.MergeLines; } set { Model.MergeLines = value; } }

        public TextDocument TemplateText { get; set; }

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
            var result = new Dictionary<string, string>();

            result.Add("--Format Parts--", "");
            result.Add("{RESOURCES}", "Resource List");
            result.Add("{ORDERS}", "Order List");
            result.Add("{ORDER TAGS}", "Order Tag List");
            result.Add("{TAXES}", "Tax List");
            result.Add("{DISCOUNTS}", "Discount List");
            result.Add("{SERVICES}", "Service List");
            result.Add("{PAYMENTS}", "Payment List");
            result.Add("{CHANGES}", "Change Payment List");

            foreach (var tagDescription in _printerService.GetTagDescriptions())
            {
                result.Add(tagDescription.Key, tagDescription.Value);
            }

            return result;
        }

        protected override void Initialize()
        {
            base.Initialize();
            TemplateText = new TextDocument(Template ?? "");
        }

        protected override void OnSave(string value)
        {
            Template = TemplateText.Text;
            base.OnSave(value);
        }
    }
}
