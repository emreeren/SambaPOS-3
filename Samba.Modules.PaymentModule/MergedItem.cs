using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Modules.PaymentModule
{
    public class MergedItem : ObservableObject
    {
        public int MenuItemId { get; set; }
        private decimal _quantity;
        public decimal Quantity { get { return _quantity; } set { _quantity = value; RaisePropertyChanged(() => Quantity); RaisePropertyChanged(() => TotalLabel); } }
        public string Description { get; set; }
        public string Label { get { return GetPaidItemsQuantity() > 0 ? string.Format("{0} ({1:#.##})", Description, GetPaidItemsQuantity()) : Description; } }
        private decimal _price;
        public decimal Price { get { return _price; } set { _price = value; NewPaidItems.ForEach(x => x.Price = value); RaisePropertyChanged(() => TotalLabel); } }
        public decimal Total { get { return (Price * Quantity) - PaidItems.Sum(x => x.Price * x.Quantity); } }
        public string TotalLabel { get { return Total > 0 ? Total.ToString("#,#0.00") : ""; } }
        public List<PaidItem> PaidItems { get; set; }
        public List<PaidItem> NewPaidItems { get; set; }
        public FontWeight FontWeight { get; set; }

        public MergedItem()
        {
            PaidItems = new List<PaidItem>();
            NewPaidItems = new List<PaidItem>();
            FontWeight = FontWeights.Normal;
        }

        private decimal GetPaidItemsQuantity()
        {
            return PaidItems.Sum(x => x.Quantity) + NewPaidItems.Sum(x => x.Quantity);
        }

        public decimal GetNewTotal()
        {
            return NewPaidItems.Sum(x => x.Quantity * x.Price);
        }

        public decimal RemainingQuantity { get { return Quantity - GetPaidItemsQuantity(); } }

        public void IncQuantity(decimal quantity)
        {
            var pitem = new PaidItem { MenuItemId = MenuItemId, Price = Price };
            NewPaidItems.Add(pitem);
            pitem.Quantity += quantity;
            FontWeight = FontWeights.Bold;
            Refresh();
        }

        public void PersistPaidItems()
        {
            foreach (var newPaidItem in NewPaidItems)
            {
                var item = newPaidItem;
                var pitem = PaidItems.SingleOrDefault(
                    x => x.MenuItemId == item.MenuItemId && x.Price == item.Price);
                if (pitem != null)
                {
                    pitem.Quantity += newPaidItem.Quantity;
                }
                else PaidItems.Add(newPaidItem);
            }

            NewPaidItems.Clear();
            FontWeight = FontWeights.Normal;
            Refresh();
        }

        public void CancelPaidItems()
        {
            NewPaidItems.Clear();
            FontWeight = FontWeights.Normal;
            Refresh();
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => Label);
            RaisePropertyChanged(() => TotalLabel);
            RaisePropertyChanged(() => FontWeight);
            RaisePropertyChanged(() => Price);
        }
    }
}