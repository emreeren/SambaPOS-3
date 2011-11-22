using System;
using System.Collections.Generic;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class PrinterTemplateViewModel : EntityViewModelBase<PrinterTemplate>
    {
        public PrinterTemplateViewModel(PrinterTemplate model)
            : base(model)
        {
        }

        public string HeaderTemplate { get { return Model.HeaderTemplate; } set { Model.HeaderTemplate = value; } }
        public string LineTemplate { get { return Model.LineTemplate; } set { Model.LineTemplate = value; } }
        public string VoidedLineTemplate { get { return Model.VoidedLineTemplate; } set { Model.VoidedLineTemplate = value; } }
        public string GiftLineTemplate { get { return Model.GiftLineTemplate; } set { Model.GiftLineTemplate = value; } }
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

        private static IDictionary<string, string> CreateDescriptions()
        {
            var result = new Dictionary<string, string>();
            result.Add(Resources.TF_TicketDate, Resources.TicketDate);
            result.Add(Resources.TF_TicketTime, Resources.TicketTime);
            result.Add(Resources.TF_DayDate, Resources.DayDate);
            result.Add(Resources.TF_DayTime, Resources.DayTime);
            result.Add(Resources.TF_UniqueTicketId, Resources.UniqueTicketId);
            result.Add(Resources.TF_TicketNumber, Resources.TicketNumber);
            result.Add(Resources.TF_TicketTag, Resources.TicketTag);
            result.Add("{DEPARTMENT}", Resources.DepartmentName);
            result.Add(Resources.TF_OptionalTicketTag, Resources.OptionalTicketTag);
            result.Add(Resources.TF_TableOrUserName, Resources.TableOrUserName);
            result.Add(Resources.TF_UserName, Resources.UserName);
            result.Add(Resources.TF_TableName, Resources.TableName);
            result.Add(Resources.TF_TicketNote, Resources.TicketNote);
            result.Add(Resources.TF_AccountName, Resources.AccountName);
            result.Add(Resources.TF_AccountAddress, Resources.AccountAddress);
            result.Add(Resources.TF_AccountPhone, Resources.AccountPhone);
            result.Add(Resources.TF_LineItemQuantity, Resources.LineItemQuantity);
            result.Add(Resources.TF_LineItemName, Resources.LineItemName);
            result.Add(Resources.TF_LineItemPrice, Resources.LineItemPrice);
            result.Add(Resources.TF_LineItemPriceCents, Resources.LineItemPriceCents);
            result.Add(Resources.TF_LineItemTotal, Resources.LineItemTotal);
            result.Add(Resources.TF_LineItemTotalAndQuantity, Resources.LineItemQuantity);
            result.Add(Resources.TF_LineItemTotalWithoutGifts, Resources.LineItemTotalWithoutGifts);
            result.Add(Resources.TF_LineItemDetails, Resources.LineItemDetails);
            result.Add(Resources.TF_LineItemDetailPrice, Resources.LineItemDetailPrice);
            result.Add(Resources.TF_LineItemDetailQuantity, Resources.TF_LineItemDetailQuantity);
            result.Add(Resources.TF_LineOrderNumber, Resources.LineOrderNumber);
            result.Add("{PRICE TAG}", Resources.LinePriceTag);
            result.Add(Resources.TF_TicketTotal, Resources.TicketTotal);
            result.Add(Resources.TF_TicketPaidTotal, Resources.TicketPaidTotal);
            result.Add("{PLAIN TOTAL}", Resources.TicketSubTotal);
            result.Add("{DISCOUNT TOTAL}", Resources.DiscountTotal);
            result.Add("{TAX TOTAL}", Resources.TaxTotal);
            result.Add("{TAX DETAILS}", Resources.TotalsGroupedByTaxTemplate);
            result.Add("{SERVICE TOTAL}", Resources.ServiceTotal);
            result.Add("{SERVICE DETAILS}", Resources.TotalsGroupedByServiceTemplate);
            result.Add(Resources.TF_TicketRemainingAmount, Resources.TicketRemainingAmount);
            result.Add(Resources.TF_RemainingAmountIfPaid, Resources.RemainingAmountIfPaid);
            result.Add("{TOTAL TEXT}", Resources.TextWrittenTotalValue);
            result.Add(Resources.TF_DiscountTotalAndTicketTotal, Resources.DiscountTotalAndTicketTotal);

            return result;
        }
    }
}
