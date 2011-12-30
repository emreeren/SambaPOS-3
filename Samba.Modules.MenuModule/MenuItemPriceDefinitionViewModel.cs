using System;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.MenuModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class MenuItemPriceDefinitionViewModel : EntityViewModelBase<MenuItemPriceDefinition>
    {
        private readonly IMenuService _menuService;

        [ImportingConstructor]
        public MenuItemPriceDefinitionViewModel(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }

        public override Type GetViewType()
        {
            return typeof(MenuItemPriceDefinitionView);
        }

        public override string GetModelTypeString()
        {
            return Resources.PriceDefinition;
        }

        protected override void OnSave(string value)
        {
            _menuService.UpdatePriceTags(Model);
            base.OnSave(value);
        }
    }
}
