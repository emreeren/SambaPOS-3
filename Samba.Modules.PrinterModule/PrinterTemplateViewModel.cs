using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using ICSharpCode.AvalonEdit.Document;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common.DataGeneration;
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
            LoadTicketTemplateCommand = new CaptionCommand<string>("", OnLoadTicketTemplate);
            LoadKitchenOrderTemplateCommand = new CaptionCommand<string>("", OnLoadKitchenOrderTemplate);
            LoadCustomerReceiptCommand = new CaptionCommand<string>("", OnLoadCustomerReceiptTemplate);
        }

        public ICaptionCommand LoadTicketTemplateCommand { get; set; }
        public ICaptionCommand LoadKitchenOrderTemplateCommand { get; set; }
        public ICaptionCommand LoadCustomerReceiptCommand { get; set; }

        public string Template { get { return Model.Template; } set { Model.Template = value; } }
        public bool MergeLines { get { return Model.MergeLines; } set { Model.MergeLines = value; } }

        private TextDocument _templateText;
        public TextDocument TemplateText
        {
            get { return _templateText; }
            set
            {
                _templateText = value;
                RaisePropertyChanged(() => TemplateText);
            }
        }

        private void OnLoadTicketTemplate(string obj)
        {
            if (string.IsNullOrEmpty(TemplateText.Text) || MessageBox.Show(string.Format(Resources.ReloadPrinterTemplateConfirmation_f, Resources.TicketTemplate), Resources.Confirmation, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                TemplateText = new TextDocument(DataCreationService.GetDefaultTicketPrintTemplate());
        }

        private void OnLoadKitchenOrderTemplate(string obj)
        {
            if (string.IsNullOrEmpty(TemplateText.Text) || MessageBox.Show(string.Format(Resources.ReloadPrinterTemplateConfirmation_f, Resources.KitchenOrderTemplate), Resources.Confirmation, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                TemplateText = new TextDocument(DataCreationService.GetDefaultKitchenPrintTemplate());
        }

        private void OnLoadCustomerReceiptTemplate(string obj)
        {
            if (string.IsNullOrEmpty(TemplateText.Text) || MessageBox.Show(string.Format(Resources.ReloadPrinterTemplateConfirmation_f, Resources.CustomerReceiptTemplate), Resources.Confirmation, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                TemplateText = new TextDocument(DataCreationService.GetDefaultCustomerReceiptTemplate());
        }

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
            result.Add("{ENTITIES}", string.Format(Resources.List_f, Resources.Entity));
            result.Add("{ORDERS}", string.Format(Resources.List_f, Resources.Order));
            result.Add("{ORDER TAGS}", string.Format(Resources.List_f, Resources.OrderTag));
            result.Add("{TAXES}", string.Format(Resources.List_f, Resources.Tax));
            result.Add("{DISCOUNTS}", string.Format(Resources.List_f, Resources.Discount));
            result.Add("{SERVICES}", string.Format(Resources.List_f, Resources.Service));
            result.Add("{PAYMENTS}", string.Format(Resources.List_f, Resources.Payment));
            result.Add("{CHANGES}", string.Format(Resources.List_f, Resources.ChangePayment));

            foreach (var tagDescription in _printerService.GetTagDescriptions())
            {
                result.Add(tagDescription.Key, tagDescription.Value);
            }

            result.Add("--" + Resources.OrderGrouping + "--", "");
            result.Add("[ORDERS GROUP|ORDER STATE:x]", Resources.OrderState);
            result.Add("[ORDERS GROUP|ORDER TAG:x]", Resources.OrderTag);
            result.Add("[ORDERS GROUP|PRODUCT GROUP]", Resources.GroupCode);
            result.Add("[ORDERS GROUP|PRODUCT TAG]", Resources.ProductTag);
            result.Add("[ORDERS GROUP|BARCODE]", Resources.ProductTag);
            result.Add("[ORDERS FOOTER]", Resources.GroupFooter);
            result.Add("{GROUP KEY}", Resources.GroupKey);
            result.Add("{GROUP SUM}", Resources.GroupTotal);
            result.Add("{QUANTITY SUM}", Resources.QuantityTotal);

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
