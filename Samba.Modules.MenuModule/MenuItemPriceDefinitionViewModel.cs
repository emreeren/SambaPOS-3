using System;
using System.ComponentModel.Composition;
using FluentValidation;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.MenuModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class MenuItemPriceDefinitionViewModel : EntityViewModelBase<MenuItemPriceDefinition>
    {
        private readonly IPriceListService _priceListService;

        [ImportingConstructor]
        public MenuItemPriceDefinitionViewModel(IPriceListService priceListService)
        {
            _priceListService = priceListService;
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
            _priceListService.UpdatePriceTags(Model);
            base.OnSave(value);
        }

        protected override AbstractValidator<MenuItemPriceDefinition> GetValidator()
        {
            return new MenuItemPriceDefinitionValidatior();
        }
    }

    internal class MenuItemPriceDefinitionValidatior : AbstractValidator<MenuItemPriceDefinition>
    {
        public MenuItemPriceDefinitionValidatior()
        {
            RuleFor(x => x.PriceTag).NotEmpty();
        }
    }
}
