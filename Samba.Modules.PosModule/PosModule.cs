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
        private readonly PosView _posView;
        private readonly TicketExplorerView _ticketExplorerView;
        private readonly MenuItemSelectorView _menuItemSelectorView;
        private readonly OpenTicketsView _openTicketsView;
        private readonly IRegionManager _regionManager;
        private readonly IApplicationState _applicationState;
        private readonly TicketListView _ticketListView;

        [ImportingConstructor]
        public PosModule(IRegionManager regionManager, IApplicationState applicationState,
            PosView posView, TicketListView ticketListView,
            TicketExplorerView ticketExplorerView, OpenTicketsView openTicketsView,
            MenuItemSelectorView menuItemSelectorView)
            : base(regionManager, AppScreens.TicketList)
        {
            SetNavigationCommand("POS", Resources.Common, "Images/Network.png", 10);

            _posView = posView;
            _openTicketsView = openTicketsView;
            _ticketExplorerView = ticketExplorerView;
            _menuItemSelectorView = menuItemSelectorView;
            _regionManager = regionManager;
            _applicationState = applicationState;
            _ticketListView = ticketListView;

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.AccountSelectedForTicket || x.Topic == EventTopicNames.PaymentRequestedForTicket)
                        Activate();
                });
        }

        protected override void OnInitialization()
        {
            _regionManager.Regions[RegionNames.MainRegion].Add(_posView, "PosView");
            _regionManager.Regions[RegionNames.PosMainRegion].Add(_openTicketsView, "OpenTicketsView");
            _regionManager.Regions[RegionNames.PosMainRegion].Add(_ticketListView, "TicketListView");
            _regionManager.Regions[RegionNames.PosSubRegion].Add(_menuItemSelectorView, "MenuItemSelectorView");
            _regionManager.Regions[RegionNames.PosSubRegion].Add(_ticketExplorerView, "TicketExplorerView");
            _regionManager.RegisterViewWithRegion(RegionNames.TicketOrdersRegion, typeof(TicketOrdersView));
            
        }

        protected override bool CanNavigate(string arg)
        {
            return _applicationState.IsCurrentWorkPeriodOpen;
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        public override object GetVisibleView()
        {
            return _posView;
        }
    }
}
