using System;
using System.Collections.Generic;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Modules.PrinterModule.ValueChangers;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.PrinterModule
{
    public class PrinterTemplateViewModel : EntityViewModelBase<PrinterTemplate>
    {
        public PrinterTemplateViewModel(PrinterTemplate model)
            : base(model)
        {
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

        private static IDictionary<string, string> CreateDescriptions()
        {
            var result = new Dictionary<string, string>();
            result.Add(TagNames.TicketDate, Resources.TicketDate);
            result.Add(TagNames.TicketTime, Resources.TicketTime);
            result.Add(TagNames.Date, Resources.DayDate);
            result.Add(TagNames.Time, Resources.DayTime);
            result.Add(TagNames.TicketId, Resources.UniqueTicketId);
            result.Add(TagNames.TicketNo, Resources.TicketNumber);
            result.Add(TagNames.TicketTag, Resources.TicketTag);
            result.Add(TagNames.Department, Resources.DepartmentName);
            result.Add(TagNames.TicketTag2, Resources.OptionalTicketTag);
            result.Add(TagNames.LocationUser, Resources.LocationOrUserName);
            result.Add(TagNames.UserName, Resources.UserName);
            result.Add(TagNames.Location, Resources.LocationName);
            result.Add(TagNames.Note, Resources.TicketNote);
            result.Add(TagNames.AccName, Resources.AccountName);
            result.Add(TagNames.AccAddress, Resources.AccountAddress);
            result.Add(TagNames.AccPhone, Resources.AccountPhone);
            result.Add(TagNames.Quantity, Resources.LineItemQuantity);
            result.Add(TagNames.Name, Resources.LineItemName);
            result.Add(TagNames.Price, Resources.LineItemPrice);
            result.Add(TagNames.Cents, Resources.LineItemPriceCents);
            result.Add(TagNames.Total, Resources.LineItemTotal);
            result.Add(TagNames.TotalAmount, Resources.LineItemQuantity);
            result.Add(TagNames.LineAmount, Resources.LineItemTotalWithoutGifts);
            result.Add(TagNames.Properties, Resources.LineItemDetails);
            result.Add(TagNames.PropPrice, Resources.LineItemDetailPrice);
            result.Add(TagNames.PropQuantity, Resources.LineItemDetailQuantity);
            result.Add(TagNames.OrderNo, Resources.LineOrderNumber);
            result.Add(TagNames.PriceTag, Resources.LinePriceTag);
            result.Add(TagNames.TicketTotal, Resources.TicketTotal);
            result.Add(TagNames.PaymentTotal, Resources.TicketPaidTotal);
            result.Add(TagNames.PlainTotal, Resources.TicketSubTotal);
            result.Add(TagNames.DiscountTotal, Resources.DiscountTotal);
            result.Add(TagNames.TaxTotal, Resources.TaxTotal);
            result.Add(TagNames.TaxDetails, Resources.TotalsGroupedByTaxTemplate);
            result.Add(TagNames.ServiceTotal, Resources.ServiceTotal);
            result.Add(TagNames.ServiceDetails, Resources.TotalsGroupedByServiceTemplate);
            result.Add(TagNames.Balance, Resources.TicketRemainingAmount);
            result.Add(TagNames.IfPaid, Resources.RemainingAmountIfPaid);
            result.Add(TagNames.TotalText, Resources.TextWrittenTotalValue);
            result.Add(TagNames.IfDiscount, Resources.DiscountTotalAndTicketTotal);

            return result;
        }
    }
}
