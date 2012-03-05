using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.MenuModule
{
    [Export(typeof(IPriceListService))]
    class PriceListService : IPriceListService
    {
        [ImportingConstructor]
        public PriceListService()
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

        public void UpdatePriceTags(MenuItemPriceDefinition model)
        {
            if (model.Id > 0)
            {
                var mip = Dao.Single<MenuItemPriceDefinition>(x => x.Id == model.Id);
                if (mip.PriceTag != model.PriceTag)
                {
                    using (var workspace = WorkspaceFactory.Create())
                    {
                        workspace.All<MenuItemPrice>(x => x.PriceTag == mip.PriceTag)
                            .ToList()
                            .ForEach(x => x.PriceTag = model.PriceTag);
                        workspace.CommitChanges();
                    }
                }
            }
        }

        public IEnumerable<string> GetTags()
        {
            var tags = Dao.Select<MenuItemPriceDefinition, string>(x => x.PriceTag, x => x.Id > 0).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
            tags.Insert(0, null);
            return tags;
        }

        public IEnumerable<PriceData> CreatePrices()
        {
            return Dao.Query<MenuItem>(x => x.Portions.Select(y => y.Prices))
                                .SelectMany(y => y.Portions, (mi, pt) => new PriceData(pt, mi.Name));
        }

        public void UpdatePrices(IEnumerable<PriceData> prices)
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
