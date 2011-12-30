using System.ComponentModel.Composition;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.MenuModule
{
    [Export,PartCreationPolicy(CreationPolicy.NonShared)]
    class MenuItemPriceDefinitionListViewModel : EntityCollectionViewModelBase<MenuItemPriceDefinitionViewModel, MenuItemPriceDefinition>
    {
        private readonly IMenuService _menuService;

        [ImportingConstructor]
        public MenuItemPriceDefinitionListViewModel(IMenuService menuService)
        {
            _menuService = menuService;
        }

        protected override void BeforeDeleteItem(MenuItemPriceDefinition item)
        {
            _menuService.DeleteMenuItemPricesByPriceTag(item.PriceTag);
        }
    }
}
