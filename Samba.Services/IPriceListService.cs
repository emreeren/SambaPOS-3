using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Persistance;
using Samba.Persistance.Common;

namespace Samba.Services
{
    public interface IPriceListService
    {
        void DeleteMenuItemPricesByPriceTag(string priceTag);
        void UpdatePriceTags(MenuItemPriceDefinition model);
        IEnumerable<string> GetTags();
        IEnumerable<PriceData> CreatePrices();
        void UpdatePrices(IList<PriceData> prices);
    }
}
