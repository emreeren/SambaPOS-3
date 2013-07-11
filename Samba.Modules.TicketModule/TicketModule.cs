using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.TicketModule
{
    [ModuleExport(typeof(TicketModule))]
    public class TicketModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IUserService _userService;
        private readonly TicketExplorerView _ticketExplorerView;
        private readonly TicketExplorerViewModel _ticketExplorerViewModel;

        [ImportingConstructor]
        public TicketModule(IRegionManager regionManager, IUserService userService, TicketExplorerView ticketExplorerView, TicketExplorerViewModel ticketExplorerViewModel)
            : base(regionManager, AppScreens.TicketListView)
        {
            _regionManager = regionManager;
            _userService = userService;

            _ticketExplorerView = ticketExplorerView;
            _ticketExplorerViewModel = ticketExplorerViewModel;
            _ticketExplorerViewModel.TicketAction = () => OnNavigate("");

            AddDashboardCommand<EntityCollectionViewModelBase<TicketTypeViewModel, TicketType>>(Resources.TicketType.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TicketTagGroupViewModel, TicketTagGroup>>(Resources.TicketTag.ToPlural(), Resources.Tickets, 35);
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
            PermissionRegistry.RegisterPermission(PermissionNames.DisplayOtherWaitersTickets, PermissionCategories.Ticket, Resources.CanDisplayOtherWaitersTickets);

            SetNavigationCommand(Resources.Tickets, Resources.Common, "Images/note.png", 20);

            ticketExplorerView.DataContext = ticketExplorerViewModel;
        }

        protected override bool CanNavigate(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.DisplayOldTickets);
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            _ticketExplorerViewModel.Refresh();
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(TicketExplorerView));
        }

        public override object GetVisibleView()
        {
            return _ticketExplorerView;
        }
    }
}
