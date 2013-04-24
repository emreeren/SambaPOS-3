using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Persistance;
using Samba.Persistance.Common;

namespace Samba.Services.Implementations.MenuModule
{
    [Export(typeof(IPriceListService))]
    class PriceListService : IPriceListService
    {
        private readonly IPriceListDao _priceListDao;

        [ImportingConstructor]
        public PriceListService(IPriceListDao priceListDao)
        {
            _priceListDao = priceListDao;
        }

        public void DeleteMenuItemPricesByPriceTag(string priceTag)
        {
            _priceListDao.DeleteMenuItemPricesByPriceTag(priceTag);
        }

        public void UpdatePriceTags(MenuItemPriceDefinition model)
        {
            if (model.Id > 0)
            {
                _priceListDao.UpdatePriceTags(model.Id, model.PriceTag);
            }
        }

        public IEnumerable<string> GetTags()
        {
            var tags = _priceListDao.GetTags().ToList();
            tags.Insert(0, null);
            return tags;
        }

        public IEnumerable<PriceData> CreatePrices()
        {
            return _priceListDao.CreatePrices();
        }

        public void UpdatePrices(IList<PriceData> prices)
        {
            _priceListDao.UpdatePrices(prices);
        }
    }
}
