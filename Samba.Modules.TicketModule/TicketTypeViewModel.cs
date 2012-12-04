using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Omu.ValueInjecter;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.DaoClasses;
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

        [ImportingConstructor]
        public TicketTypeViewModel(IMenuService menuService, ICacheDao cacheDao)
        {
            _menuService = menuService;
            _cacheDao = cacheDao;
            AddResourceTypeCommand = new CaptionCommand<string>(Resources.Add, OnAddResourceType);
        }

        public CaptionCommand<string> AddResourceTypeCommand { get; set; }
        public int ScreenMenuId { get { return Model.ScreenMenuId; } set { Model.ScreenMenuId = value; } }
        public Numerator TicketNumerator { get { return Model.TicketNumerator; } set { Model.TicketNumerator = value; } }
        public Numerator OrderNumerator { get { return Model.OrderNumerator; } set { Model.OrderNumerator = value; } }
        public AccountTransactionType SaleTransactionType { get { return Model.SaleTransactionType; } set { Model.SaleTransactionType = value; } }

        private IEnumerable<ResourceType> _resourceTypes;
        public IEnumerable<ResourceType> ResourceTypes
        {
            get { return _resourceTypes ?? (_resourceTypes = _cacheDao.GetResourceTypes()); }
        }

        private ObservableCollection<ResourceTypeAssignment> _resourceTypeAssignments;
        public ObservableCollection<ResourceTypeAssignment> ResourceTypeAssignments
        {
            get { return _resourceTypeAssignments ?? (_resourceTypeAssignments = new ObservableCollection<ResourceTypeAssignment>(Model.ResourceTypeAssignments)); }
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

        private void OnAddResourceType(string obj)
        {
            var selectedItems = Model.ResourceTypeAssignments;
            var values = ResourceTypes.Where(x => selectedItems.All(y => y.ResourceTypeName != x.Name))
                .Select(x => new ResourceTypeAssignment { ResourceTypeName = x.Name, ResourceTypeId = x.Id })
                .ToList<IOrderable>();
            var selectedValues = InteractionService.UserIntraction.ChooseValuesFrom(
                values,
                selectedItems.ToList<IOrderable>(),
                Resources.ResourceType.ToPlural(),
                string.Format(Resources.SelectItemsFor_f, Resources.ResourceType.ToPlural(), Model.Name,
                              Resources.TicketType),
                Resources.ResourceType,
                Resources.ResourceType.ToPlural());

            Model.InjectFrom<EntityInjection>(new { ResourceTypeAssignments = selectedValues.Cast<ResourceTypeAssignment>().ToList() });
            _resourceTypeAssignments = null;
            RaisePropertyChanged(() => ResourceTypeAssignments);
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
            RuleFor(x => x.SaleTransactionType.DefaultSourceAccountId).GreaterThan(0).When(x => x.SaleTransactionType != null);
            RuleFor(x => x.TicketNumerator).NotEqual(x => x.OrderNumerator).When(x => x.TicketNumerator != null);
        }
    }
}