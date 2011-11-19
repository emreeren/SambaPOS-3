using System;

namespace Samba.Domain.Models.Tickets
{
    public class OrderTagValue
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Price { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Quantity { get; set; }
        public int OrderTagGroupId { get; set; }
        public int MenuItemId { get; set; }
        public bool AddTagPriceToOrderPrice { get; set; }
        public string PortionName { get; set; }
        public int TagAction { get; set; }

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
    }
}
