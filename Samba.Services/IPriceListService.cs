using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;

namespace Samba.Services
{
    public class PriceData
    {
        public MenuItemPortion Portion { get; set; }
        public string Name { get; set; }

        public PriceData(MenuItemPortion portion, string name)
        {
            Portion = portion;
            Name = name;
        }
    }

    public interface IPriceListService
    {
        void DeleteMenuItemPricesByPriceTag(string priceTag);
        void UpdatePriceTags(MenuItemPriceDefinition model);
        IEnumerable<string> GetTags();
        IEnumerable<PriceData> CreatePrices();
        void UpdatePrices(IEnumerable<PriceData> prices);
    }
}
