using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Validation;
using Samba.Localization.Properties;
using Samba.Persistance.Common;
using Samba.Persistance.Data;

namespace Samba.Persistance.Implementations
{
    [Export(typeof(IPriceListDao))]
    class PriceListDao : IPriceListDao
    {
        [ImportingConstructor]
        public PriceListDao()
        {
            ValidatorRegistry.RegisterSaveValidator(new MenuItemPriceDefinitionSaveValidator());
        }

        public void DeleteMenuItemPricesByPriceTag(string priceTag)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                workspace.Delete<MenuItemPrice>(x => x.PriceTag == priceTag);
                workspace.CommitChanges();
            }
        }

        public void UpdatePriceTags(int id, string priceTag)
        {
            var mip = Dao.Single<MenuItemPriceDefinition>(x => x.Id == id);
            if (mip.PriceTag != priceTag)
            {
                using (var workspace = WorkspaceFactory.Create())
                {
                    workspace.All<MenuItemPrice>(x => x.PriceTag == mip.PriceTag)
                        .ToList()
                        .ForEach(x => x.PriceTag = priceTag);
                    workspace.CommitChanges();
                }
            }
        }

        public IEnumerable<string> GetTags()
        {
            return Dao.Select<MenuItemPriceDefinition, string>(x => x.PriceTag, x => x.Id > 0).Distinct().Where(x => !string.IsNullOrEmpty(x));
        }

        public IEnumerable<PriceData> CreatePrices()
        {
            return Dao.Query<MenuItem>(x => x.Portions.Select(y => y.Prices))
                                .SelectMany(y => y.Portions, (mi, pt) => new PriceData(pt, mi.Name));
        }

        public void UpdatePrices(IList<PriceData> prices)
        {
            using (var wp = WorkspaceFactory.Create())
            {
                var ids = prices.Select(x => x.Portion.Id);
                var portions = wp.All<MenuItemPortion>(x => ids.Contains(x.Id));
                foreach (var menuItemPortion in portions)
                {
                    var id = menuItemPortion.Id;
                    var portion = prices.Single(x => x.Portion.Id == id).Portion;
                    menuItemPortion.UpdatePrices(portion.Prices);
                }
                wp.CommitChanges();
            }
        }
    }

    public class MenuItemPriceDefinitionSaveValidator : SpecificationValidator<MenuItemPriceDefinition>
    {
        public override string GetErrorMessage(MenuItemPriceDefinition model)
        {
            if (Dao.Exists<MenuItemPriceDefinition>(x => x.PriceTag.ToLower() == model.PriceTag.ToLower() && model.Id != x.Id))
                return string.Format(Resources.ThereIsAnotherPriceDefinition_f, model.PriceTag);
            return "";
        }
    }
}
