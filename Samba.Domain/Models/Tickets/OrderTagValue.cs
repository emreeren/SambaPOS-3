using System;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderTagValue : Value
    {
        public OrderTagValue()
        {
            Name = "";
        }

        public int OrderId { get; set; }
        public int TicketId { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public decimal Price { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Quantity { get; set; }
        public string OrderTagGroupName { get; set; }
        public int OrderTagGroupId { get; set; }
        public int MenuItemId { get; set; }
        public bool AddTagPriceToOrderPrice { get; set; }
        public string PortionName { get; set; }
        public bool SubValue { get; set; }
        internal bool NewTag { get; set; }
        public string OrderKey { get; set; }
        public bool FreeTag { get; set; }

        public void UpdatePrice(bool taxIncluded, decimal taxRate, decimal orderTagPrice)
        {
            Price = orderTagPrice;
            if (taxIncluded && taxRate > 0)
            {
                Price = orderTagPrice / ((100 + taxRate) / 100);
                Price = decimal.Round(Price, 2);
                TaxAmount = orderTagPrice - Price;
            }
            else if (taxRate > 0) TaxAmount = (orderTagPrice * taxRate) / 100;
            else TaxAmount = 0;
        }

        private string _shortName;
        public string ShortName
        {
            get { return _shortName ?? (_shortName = ToShort(Name)); }
        }

        private static OrderTagValue _empty;
        public static OrderTagValue Empty
        {
            get { return _empty ?? (_empty = new OrderTagValue { Name = "" }); }
        }

        private string ToShort(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            if (Name.Length < 3) return name;
            return name.Contains(" ") ? string.Join("", name.Split(' ').Select(x => char.IsNumber(x.ElementAt(0)) ? x : x.ElementAt(0).ToString())) : Name.Substring(0, 2);
        }
    }
}
