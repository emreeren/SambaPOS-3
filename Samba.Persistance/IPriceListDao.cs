using System.Collections.Generic;
using Samba.Persistance.Common;

namespace Samba.Persistance
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
