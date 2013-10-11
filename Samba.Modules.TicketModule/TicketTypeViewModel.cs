using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Omu.ValueInjecter;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Injection;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class TicketTypeViewModel : EntityViewModelBase<TicketType>
    {
        private readonly IMenuService _menuService;
        private readonly ICacheDao _cacheDao;
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public TicketTypeViewModel(IMenuService menuService, ICacheDao cacheDao, ISettingService settingService)
        {
            _menuService = menuService;
            _cacheDao = cacheDao;
            _settingService = settingService;
            AddEntityTypeCommand = new CaptionCommand<string>(Resources.Select, OnAddEntityType);
            AddMenuCommand = new CaptionCommand<string>(Resources.Select, OnAddMenu);
        }

        public CaptionCommand<string> AddEntityTypeCommand { get; set; }
        public CaptionCommand<string> AddMenuCommand { get; set; }
        public int ScreenMenuId { get { return Model.ScreenMenuId; } set { Model.ScreenMenuId = value; } }
        public Numerator TicketNumerator { get { return Model.TicketNumerator; } set { Model.TicketNumerator = value; } }
        public Numerator OrderNumerator { get { return Model.OrderNumerator; } set { Model.OrderNumerator = value; } }
        public AccountTransactionType SaleTransactionType { get { return Model.SaleTransactionType; } set { Model.SaleTransactionType = value; } }
        public bool TaxIncluded { get { return Model.TaxIncluded; } set { Model.TaxIncluded = value; } }

        private IEnumerable<EntityType> _entityTypes;
        public IEnumerable<EntityType> EntityTypes
        {
            get { return _entityTypes ?? (_entityTypes = _cacheDao.GetEntityTypes()); }
        }

        private IEnumerable<ScreenMenu> _screenMenus;
        public IEnumerable<ScreenMenu> ScreenMenus
        {
            get { return _screenMenus ?? (_screenMenus = _menuService.GetScreenMenus()); }
            set { _screenMenus = value; }
        }

        private IEnumerable<Numerator> _numerators;
        public IEnumerable<Numerator> Numerators { get { return _numerators ?? (_numerators = Workspace.All<Numerator>()); } set { _numerators = value; } }

        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes { get { return _accountTransactionTypes ?? (_accountTransactionTypes = Workspace.All<AccountTransactionType>()); } }

        private ObservableCollection<MenuAssignmentViewModel> _menuAssignments;
        public ObservableCollection<MenuAssignmentViewModel> MenuAssignments
        {
            get { return _menuAssignments ?? (_menuAssignments = new ObservableCollection<MenuAssignmentViewModel>(GetMenuAssignmentViewModels())); }
        }

        private ObservableCollection<EntityTypeAssignment> _entityTypeAssignments;
        public ObservableCollection<EntityTypeAssignment> EntityTypeAssignments
        {
            get { return _entityTypeAssignments ?? (_entityTypeAssignments = new ObservableCollection<EntityTypeAssignment>(Model.EntityTypeAssignments)); }
        }

        private IEnumerable<MenuAssignmentViewModel> GetMenuAssignmentViewModels()
        {
            return Model.MenuAssignments.Select(x => new MenuAssignmentViewModel(x, ScreenMenus));
        }

        private void OnAddMenu(string obj)
        {
            var selectedItems = Model.MenuAssignments;
            var values = _settingService.GetTerminals().Where(x => selectedItems.All(y => y.TerminalId != x.Id))
                .Select(x => new MenuAssignment { TerminalName = x.Name, TerminalId = x.Id })
                .ToList<IOrderable>();
            var selectedValues = InteractionService.UserIntraction.ChooseValuesFrom(
                values,
                selectedItems.ToList<IOrderable>(),
                Resources.Terminal.ToPlural(),
                string.Format(Resources.SelectItemsFor_f, Resources.Terminal.ToPlural(), Model.Name,
                              Resources.TicketType),
                Resources.Terminal,
                Resources.Terminal.ToPlural());

            Model.InjectFrom<EntityInjection>(new { MenuAssignments = selectedValues.Cast<MenuAssignment>().ToList() });
            _menuAssignments = null;
            RaisePropertyChanged(() => MenuAssignments);
        }

        private void OnAddEntityType(string obj)
        {
            var selectedItems = Model.EntityTypeAssignments;
            var values = EntityTypes.Where(x => selectedItems.All(y => y.EntityTypeName != x.Name))
                .Select(x => new EntityTypeAssignment { EntityTypeName = x.Name, EntityTypeId = x.Id })
                .ToList<IOrderable>();
            var selectedValues = InteractionService.UserIntraction.ChooseValuesFrom(
                values,
                selectedItems.ToList<IOrderable>(),
                Resources.EntityType.ToPlural(),
                string.Format(Resources.SelectItemsFor_f, Resources.EntityType.ToPlural(), Model.Name,
                              Resources.TicketType),
                Resources.EntityType,
                Resources.EntityType.ToPlural());

            Model.InjectFrom<EntityInjection>(new { EntityTypeAssignments = selectedValues.Cast<EntityTypeAssignment>().ToList() });
            _entityTypeAssignments = null;
            RaisePropertyChanged(() => EntityTypeAssignments);
        }

        public override string GetModelTypeString()
        {
            return Resources.TicketType;
        }

        public override Type GetViewType()
        {
            return typeof(TicketTypeView);
        }

        protected override AbstractValidator<TicketType> GetValidator()
        {
            return new TicketTypeValidator();
        }
    }

    internal class TicketTypeValidator : EntityValidator<TicketType>
    {
        public TicketTypeValidator()
        {
            RuleFor(x => x.TicketNumerator).NotNull();
            RuleFor(x => x.OrderNumerator).NotNull();
            RuleFor(x => x.SaleTransactionType).NotNull();
            RuleFor(x => x.TicketNumerator).NotEqual(x => x.OrderNumerator).When(x => x.TicketNumerator != null);
        }
    }
}