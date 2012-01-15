using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule
{
    [ModuleExport(typeof(TicketModule))]
    public class TicketModule : VisibleModuleBase
    {
        readonly IRegionManager _regionManager;
        private readonly TicketEditorView _ticketEditorView;
        private readonly IApplicationState _applicationState;
        private readonly TicketExplorerView _ticketExplorerView;
        private readonly MenuItemSelectorView _menuItemSelectorView;
        private readonly SelectedOrdersView _selectedOrdersView;
        private readonly OpenTicketsView _openTicketsView;

        [ImportingConstructor]
        public TicketModule(IRegionManager regionManager, TicketEditorView ticketEditorView,
            IApplicationState applicationState, TicketExplorerView ticketExplorerView,
            MenuItemSelectorView menuItemSelectorView, SelectedOrdersView selectedOrdersView,
            OpenTicketsView openTicketsView)
            : base(regionManager, AppScreens.TicketList)
        {
            SetNavigationCommand("POS", Resources.Common, "Images/Network.png", 10);

            _regionManager = regionManager;
            _ticketEditorView = ticketEditorView;
            _ticketExplorerView = ticketExplorerView;
            _applicationState = applicationState;
            _menuItemSelectorView = menuItemSelectorView;
            _selectedOrdersView = selectedOrdersView;
            _openTicketsView = openTicketsView;

            AddDashboardCommand<EntityCollectionViewModelBase<TicketTemplateViewModel, TicketTemplate>>(Resources.TicketTemplates, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TicketTagGroupViewModel, TicketTagGroup>>(Resources.TicketTags, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<OrderTagGroupViewModel, OrderTagGroup>>(Resources.OrderTags, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<OrderTagTemplateViewModel, OrderTagTemplate>>(Resources.OrderTagTemplates, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<ServiceTemplateViewModel, ServiceTemplate>>(Resources.ServiceTemplates, Resources.Products, 35);

            PermissionRegistry.RegisterPermission(PermissionNames.AddItemsToLockedTickets, PermissionCategories.Ticket, Resources.CanReleaseTicketLock);
            PermissionRegistry.RegisterPermission(PermissionNames.RemoveTicketTag, PermissionCategories.Ticket, Resources.CanRemoveTicketTag);
            PermissionRegistry.RegisterPermission(PermissionNames.GiftItems, PermissionCategories.Ticket, Resources.CanGiftItems);
            PermissionRegistry.RegisterPermission(PermissionNames.VoidItems, PermissionCategories.Ticket, Resources.CanVoidItems);
            PermissionRegistry.RegisterPermission(PermissionNames.MoveOrders, PermissionCategories.Ticket, Resources.CanMoveTicketLines);
            PermissionRegistry.RegisterPermission(PermissionNames.MergeTickets, PermissionCategories.Ticket, Resources.CanMergeTickets);
            PermissionRegistry.RegisterPermission(PermissionNames.DisplayOldTickets, PermissionCategories.Ticket, Resources.CanDisplayOldTickets);
            PermissionRegistry.RegisterPermission(PermissionNames.MoveUnlockedOrders, PermissionCategories.Ticket, Resources.CanMoveUnlockedTicketLines);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeExtraProperty, PermissionCategories.Ticket, Resources.CanUpdateExtraModifiers);

            PermissionRegistry.RegisterPermission(PermissionNames.MakePayment, PermissionCategories.Payment, Resources.CanGetPayment);
            PermissionRegistry.RegisterPermission(PermissionNames.MakeFastPayment, PermissionCategories.Payment, Resources.CanGetFastPayment);
            PermissionRegistry.RegisterPermission(PermissionNames.MakeDiscount, PermissionCategories.Payment, Resources.CanMakeDiscount);
            PermissionRegistry.RegisterPermission(PermissionNames.RoundPayment, PermissionCategories.Payment, Resources.CanRoundTicketTotal);
            PermissionRegistry.RegisterPermission(PermissionNames.FixPayment, PermissionCategories.Payment, Resources.CanFlattenTicketTotal);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.AccountSelectedForTicket || x.Topic == EventTopicNames.PaymentRequestedForTicket)
                        Activate();
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.ActivateTicketView || x.Topic == EventTopicNames.DisplayTicketView)
                        Activate();
                });
        }

        protected override bool CanNavigate(string arg)
        {
            return _applicationState.IsCurrentWorkPeriodOpen;
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicketView);
        }

        public override object GetVisibleView()
        {
            return _ticketEditorView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.TicketRegion, typeof(TicketListView));
            _regionManager.RegisterViewWithRegion(RegionNames.TicketOrdersRegion, typeof(TicketOrdersView));
            _regionManager.Regions[RegionNames.MainRegion].Add(_ticketEditorView, "TicketEditorView");
            _regionManager.Regions[RegionNames.TicketSubRegion].Add(_selectedOrdersView, "SelectedOrdersView");
            _regionManager.Regions[RegionNames.TicketSubRegion].Add(_menuItemSelectorView, "MenuItemSelectorView");
            _regionManager.Regions[RegionNames.TicketSubRegion].Add(_ticketExplorerView, "TicketExplorerView");
            _regionManager.Regions[RegionNames.OpenTicketRegion].Add(_openTicketsView, "OpenTicketView");

        }
    }
}
