using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FluentValidation;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class DepartmentViewModel : EntityViewModelBase<Department>
    {
        private readonly IMenuService _menuService;
        private readonly IPriceListService _priceListService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public DepartmentViewModel(IMenuService menuService, IPriceListService priceListService, ICacheService cacheService)
        {
            _menuService = menuService;
            _priceListService = priceListService;
            _cacheService = cacheService;
        }

        private readonly IList<string> _ticketCreationMethods = new[] { string.Format(Resources.Select_f, Resources.Entity), string.Format(Resources.Create_f, Resources.Ticket) };
        public IList<string> TicketCreationMethods { get { return _ticketCreationMethods; } }
        public string TicketCreationMethod { get { return _ticketCreationMethods[Model.TicketCreationMethod]; } set { Model.TicketCreationMethod = _ticketCreationMethods.IndexOf(value); } }

        public IEnumerable<string> PriceTags { get { return _priceListService.GetTags(); } }
        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }

        private IEnumerable<TicketType> _ticketTypes;
        public IEnumerable<TicketType> TicketTypes
        {
            get { return _ticketTypes ?? (_ticketTypes = _cacheService.GetTicketTypes()); }
        }

        public int TicketTypeId { get { return Model.TicketTypeId; } set { Model.TicketTypeId = value; } }

        private IEnumerable<ScreenMenu> _screenMenus;
        public IEnumerable<ScreenMenu> ScreenMenus
        {
            get { return _screenMenus ?? (_screenMenus = _menuService.GetScreenMenus()); }
        }

        public int? ScreenMenuId { get { return Model.ScreenMenuId; } set { Model.ScreenMenuId = value.GetValueOrDefault(0); } }

        private IEnumerable<Warehouse> _warehouses;
        public IEnumerable<Warehouse> Warehouses
        {
            get { return _warehouses ?? (_warehouses = Workspace.All<Warehouse>()); }
        }

        public int WarehouseId { get { return Model.WarehouseId; } set { Model.WarehouseId = value; } }

        protected override AbstractValidator<Department> GetValidator()
        {
            return new DepartmentValidator();
        }

        public override Type GetViewType()
        {
            return typeof(DepartmentView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Department;
        }
    }

    internal class DepartmentValidator : EntityValidator<Department>
    {
        public DepartmentValidator()
        {
            RuleFor(x => x.TicketTypeId).GreaterThan(0);
            RuleFor(x => x.WarehouseId).GreaterThan(0);
        }
    }

}
