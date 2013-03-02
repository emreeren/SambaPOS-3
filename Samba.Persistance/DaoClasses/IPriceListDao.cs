using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Persistance.DaoClasses
{
    public interface IPriceListDao
    {
        void DeleteMenuItemPricesByPriceTag(string priceTag);
        void UpdatePriceTags(int id, string priceTag);
        IEnumerable<string> GetTags();
        IEnumerable<PriceData> CreatePrices();
        void UpdatePrices(IList<PriceData> prices);
    }
}
