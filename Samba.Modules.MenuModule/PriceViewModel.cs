using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common;

namespace Samba.Modules.MenuModule
{
    public class PriceViewModel : ObservableObject
    {
        public MenuItemPortion Model { get; set; }
        public string ItemName { get; set; }
        public string PortionName { get { return Model.Name; } }

        private readonly IList<MenuItemPriceViewModel> _additionalPrices;
        public IList<MenuItemPriceViewModel> AdditionalPrices
        {
            get { return _additionalPrices; }
        }

        public decimal this[int index]
        {
            get { return AdditionalPrices[index].Price; }
            set
            {
                AdditionalPrices[index].Price = value;
                IsChanged = true;
            }
        }

        private bool _isChanged;
        public bool IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
                RaisePropertyChanged(() => IsChanged);
            }
        }

        public PriceViewModel(MenuItemPortion model, string itemName, IEnumerable<string> tags)
        {
            Model = model;
            ItemName = itemName;
            _additionalPrices = new List<MenuItemPriceViewModel>();
            tags.ToList().ForEach(x =>
                {
                    var pr = model.Prices.SingleOrDefault(y => y.PriceTag == x);
                    if (pr != null) _additionalPrices.Add(new MenuItemPriceViewModel(pr));
                });
        }

        public void AddPrice(string tag)
        {
            var pr = new MenuItemPrice { PriceTag = tag };
            Model.Prices.Add(pr);
            AdditionalPrices.Add(new MenuItemPriceViewModel(pr));
        }
    }
}
