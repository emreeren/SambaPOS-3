using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [ModuleExport(typeof(TicketModule))]
    public class TicketModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly TicketModuleView _ticketModuleView;

        [ImportingConstructor]
        public TicketModule(IRegionManager regionManager, TicketModuleView ticketModuleView)
            : base(regionManager, AppScreens.TicketListView)
        {
            _regionManager = regionManager;
            _ticketModuleView = ticketModuleView;
            AddDashboardCommand<EntityCollectionViewModelBase<TicketTypeViewModel, TicketType>>(Resources.TicketType.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TicketTagGroupViewModel, TicketTagGroup>>(Resources.TicketTag.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<OrderTagTemplateViewModel, OrderTagTemplate>>(Resources.OrderTagTemplate.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<OrderTagGroupViewModel, OrderTagGroup>>(Resources.OrderTag.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<PaymentTypeViewModel, PaymentType>>(Resources.PaymentType.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<ChangePaymentTypeViewModel, ChangePaymentType>>(Resources.ChangePaymentType.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<CalculationTypeViewModel, CalculationType>>(Resources.CalculationType.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<CalculationSelectorViewModel, CalculationSelector>>(Resources.CalculationSelector.ToPlural(), Resources.Tickets, 35);

            PermissionRegistry.RegisterPermission(PermissionNames.AddItemsToLockedTickets, PermissionCategories.Ticket, Resources.CanReleaseTicketLock);
            PermissionRegistry.RegisterPermission(PermissionNames.RemoveTicketTag, PermissionCategories.Ticket, Resources.CanRemoveTicketTag);
            PermissionRegistry.RegisterPermission(PermissionNames.MoveOrders, PermissionCategories.Ticket, Resources.CanMoveTicketLines);
            PermissionRegistry.RegisterPermission(PermissionNames.MergeTickets, PermissionCategories.Ticket, Resources.CanMergeTickets);
            PermissionRegistry.RegisterPermission(PermissionNames.DisplayOldTickets, PermissionCategories.Ticket, Resources.CanDisplayOldTickets);
            PermissionRegistry.RegisterPermission(PermissionNames.MoveUnlockedOrders, PermissionCategories.Ticket, Resources.CanMoveUnlockedTicketLines);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeExtraProperty, PermissionCategories.Ticket, Resources.CanUpdateExtraModifiers);

            SetNavigationCommand(Resources.Tickets, Resources.Common, "Images/note.png", 20);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(TicketModuleView));
        }

        public override object GetVisibleView()
        {
            return _ticketModuleView;
        }
    }
}
