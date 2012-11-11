using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.PosModule
{
    [ModuleExport(typeof(PosModule))]
    class PosModule : VisibleModuleBase
    {
        private readonly PosView _posView;
        private readonly MenuItemSelectorView _menuItemSelectorView;
        private readonly IRegionManager _regionManager;
        private readonly IApplicationState _applicationState;
        private readonly TicketView _ticketView;
        private readonly TicketListView _ticketListView;
        private readonly TicketTagListView _ticketTagListView;

        [ImportingConstructor]
        public PosModule(IRegionManager regionManager, IApplicationState applicationState,
            PosView posView, TicketView ticketView, TicketListView ticketListView, TicketTagListView ticketTagListView,
            MenuItemSelectorView menuItemSelectorView)
            : base(regionManager, AppScreens.TicketList)
        {
            SetNavigationCommand("POS", Resources.Common, "Images/Network.png", 10);

            _posView = posView;
            _menuItemSelectorView = menuItemSelectorView;
            _regionManager = regionManager;
            _applicationState = applicationState;
            _ticketView = ticketView;
            _ticketListView = ticketListView;
            _ticketTagListView = ticketTagListView;

            EventServiceFactory.EventService.GetEvent<GenericEvent<Resource>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.PaymentRequestedForTicket) Activate();
                });
        }

        protected override void OnInitialization()
        {
            _regionManager.Regions[RegionNames.MainRegion].Add(_posView, "PosView");
            _regionManager.Regions[RegionNames.PosMainRegion].Add(_ticketView, "TicketView");
            _regionManager.Regions[RegionNames.PosMainRegion].Add(_ticketListView, "TicketListView");
            _regionManager.Regions[RegionNames.PosMainRegion].Add(_ticketTagListView, "TicketTagListView");
            _regionManager.Regions[RegionNames.PosSubRegion].Add(_menuItemSelectorView, "MenuItemSelectorView");
            _regionManager.RegisterViewWithRegion(RegionNames.TicketOrdersRegion, typeof(TicketOrdersView));
            _regionManager.RegisterViewWithRegion(RegionNames.TicketInfoRegion, typeof(TicketInfoView));
            _regionManager.RegisterViewWithRegion(RegionNames.TicketPaymentRegion, typeof (PaymentButtonView));
            _regionManager.RegisterViewWithRegion(RegionNames.TicketTotalsRegion, typeof (TicketTotalsView));
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
