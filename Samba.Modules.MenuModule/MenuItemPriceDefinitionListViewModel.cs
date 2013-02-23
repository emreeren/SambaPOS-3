using System.ComponentModel.Composition;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.MenuModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class MenuItemPriceDefinitionListViewModel : EntityCollectionViewModelBase<MenuItemPriceDefinitionViewModel, MenuItemPriceDefinition>
    {
        private readonly IPriceListService _priceListService;

        [ImportingConstructor]
        public MenuItemPriceDefinitionListViewModel(IPriceListService priceListService)
        {
            _priceListService = priceListService;
        }

        protected override void BeforeDeleteItem(MenuItemPriceDefinition item)
        {
            _priceListService.DeleteMenuItemPricesByPriceTag(item.PriceTag);
        }
    }
}
