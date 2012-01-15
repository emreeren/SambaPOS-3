using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [ModuleExport(typeof(PosModule))]
    class PosModule : VisibleModuleBase
    {
        private readonly TicketEditorView _ticketEditorView;
        private readonly TicketExplorerView _ticketExplorerView;
        private readonly MenuItemSelectorView _menuItemSelectorView;
        private readonly SelectedOrdersView _selectedOrdersView;
        private readonly IRegionManager _regionManager;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public PosModule(IRegionManager regionManager, IApplicationState applicationState,
            TicketEditorView ticketEditorView, TicketExplorerView ticketExplorerView,
            MenuItemSelectorView menuItemSelectorView, SelectedOrdersView selectedOrdersView)
            : base(regionManager, AppScreens.TicketList)
        {
            SetNavigationCommand("POS", Resources.Common, "Images/Network.png", 10);

            _ticketEditorView = ticketEditorView;
            _ticketExplorerView = ticketExplorerView;
            _menuItemSelectorView = menuItemSelectorView;
            _selectedOrdersView = selectedOrdersView;
            _regionManager = regionManager;
            _applicationState = applicationState;

            AddDashboardCommand<EntityCollectionViewModelBase<OrderTagGroupViewModel, OrderTagGroup>>(Resources.OrderTags, Resources.Tickets, 35);

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

        protected override void OnInitialization()
        {
            _regionManager.Regions[RegionNames.MainRegion].Add(_ticketEditorView, "TicketEditorView");
            _regionManager.Regions[RegionNames.TicketSubRegion].Add(_selectedOrdersView, "SelectedOrdersView");
            _regionManager.Regions[RegionNames.TicketSubRegion].Add(_menuItemSelectorView, "MenuItemSelectorView");
            _regionManager.Regions[RegionNames.TicketSubRegion].Add(_ticketExplorerView, "TicketExplorerView");
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
    }
}
