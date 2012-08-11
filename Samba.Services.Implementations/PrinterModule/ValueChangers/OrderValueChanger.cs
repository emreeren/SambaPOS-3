using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class OrderValueChanger : AbstractValueChanger<Order>
    {
        private readonly OrderTagValueChanger _orderTagValueChanger = new OrderTagValueChanger();

        public override string GetTargetTag()
        {
            return "ORDERS";
        }

        protected override string GetModelName(Order model)
        {
            return model.OrderStateGroupName;
        }

        protected override string ReplaceValues(string templatePart, Order model, PrinterTemplate template)
        {
            string result = templatePart;

            if (model != null)
            {
                result = FormatData(result, TagNames.Quantity, () => model.Quantity.ToString("#,#0.##"));
                result = FormatData(result, TagNames.Name, () => model.MenuItemName + model.GetPortionDesc());
                result = FormatData(result, TagNames.Price, () => model.Price.ToString("#,#0.00"));
                result = FormatData(result, TagNames.Total, () => model.GetItemPrice().ToString("#,#0.00"));
                result = FormatData(result, TagNames.TotalAmount, () => model.GetItemValue().ToString("#,#0.00"));
                result = FormatData(result, TagNames.Cents, () => (model.Price * 100).ToString("#,##"));
                result = FormatData(result, TagNames.LineAmount, () => model.GetTotal().ToString("#,#0.00"));
                result = FormatData(result, TagNames.OrderNo, () => model.OrderNumber.ToString());
                result = FormatData(result, TagNames.PriceTag, () => model.PriceTag);
                result = _orderTagValueChanger.Replace(template, result, model.OrderTagValues);
            }
            return result;
        }
    }
}
