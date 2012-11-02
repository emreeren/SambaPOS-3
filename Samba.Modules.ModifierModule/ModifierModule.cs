using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ModifierModule
{
    [ModuleExport(typeof(ModifierModule))]
    class ModifierModule : ModuleBase
    {
        private IEnumerable<Order> _selectedOrders;

        private readonly OrderTagGroupEditorView _selectedOrdersView;
        private readonly OrderTagGroupEditorViewModel _selectedOrdersViewModel;
        private readonly AutomationCommandSelectorView _automationCommandSelectorView;
        private readonly ProductTimerEditorView _productTimerEditorView;
        private readonly ProductTimerEditorViewModel _productTimerEditorViewModel;
        private readonly IRegionManager _regionManager;

        private readonly TicketNoteEditorView _ticketNoteEditorView;
        private readonly TicketNoteEditorViewModel _ticketNoteEditorViewModel;
        private readonly TicketTagEditorView _ticketTagEditorView;
        private readonly TicketTagEditorViewModel _ticketTagEditorViewModel;
        private readonly OrderStateEditorView _orderStateEditorView;
        private readonly OrderTagEditorView _orderTagEditorView;

        [ImportingConstructor]
        public ModifierModule(IRegionManager regionManager, IUserService userService,
            TicketNoteEditorView ticketNoteEditorView, TicketNoteEditorViewModel ticketNoteEditorViewModel,
            TicketTagEditorView ticketTagEditorView, TicketTagEditorViewModel ticketTagEditorViewModel,
            OrderStateEditorView orderStateEditorView, OrderTagEditorView orderTagEditorView,
            OrderTagGroupEditorView selectedOrdersView, OrderTagGroupEditorViewModel selectedOrdersViewModel,
            AutomationCommandSelectorView automationCommandSelectorView, AutomationCommandSelectorViewModel automationCommandSelectorViewModel,
            ProductTimerEditorView productTimerEditorView, ProductTimerEditorViewModel productTimerEditorViewModel)
        {
            _selectedOrdersView = selectedOrdersView;
            _selectedOrdersViewModel = selectedOrdersViewModel;
            _automationCommandSelectorView = automationCommandSelectorView;
            _productTimerEditorView = productTimerEditorView;
            _productTimerEditorViewModel = productTimerEditorViewModel;
            _ticketNoteEditorView = ticketNoteEditorView;
            _ticketNoteEditorViewModel = ticketNoteEditorViewModel;
            _ticketTagEditorView = ticketTagEditorView;
            _ticketTagEditorViewModel = ticketTagEditorViewModel;
            _orderStateEditorView = orderStateEditorView;
            _orderTagEditorView = orderTagEditorView;

            _regionManager = regionManager;

            EventServiceFactory.EventService.GetEvent<GenericEvent<SelectedOrdersData>>().Subscribe(OnSelectedOrdersDataEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnDisplayTicketDetailsScreen);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagData>>().Subscribe(OnTicketTagDataSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderStateData>>().Subscribe(OnOrderStateDataSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagDataSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnTicketEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<AutomationCommand>>().Subscribe(OnAutomationCommandEvent);
        }

        private void OnAutomationCommandEvent(EventParameters<AutomationCommand> obj)
        {
            if(obj.Topic == EventTopicNames.SelectAutomationCommandValue)
            {
                DisplayAutomationCommandValueSelector();
            }
        }

        private void OnOrderTagDataSelected(EventParameters<OrderTagData> obj)
        {
            if (obj.Topic == EventTopicNames.SelectOrderTag)
            {
                DisplayOrderTagEditor();
            }
        }

        private void OnOrderStateDataSelected(EventParameters<OrderStateData> obj)
        {
            if (obj.Topic == EventTopicNames.SelectOrderState)
            {
                DisplayOrderStateEditor();
            }
        }

        private void OnTicketTagDataSelected(EventParameters<TicketTagData> obj)
        {
            if (obj.Topic == EventTopicNames.SelectTicketTag)
            {
                var isTagSelected = _ticketTagEditorViewModel.TicketTagSelected(obj.Value.Ticket, obj.Value.TicketTagGroup);
                if (!isTagSelected) DisplayTicketTagEditor();
            }
        }

        private void OnTicketEvent(EventParameters<Ticket> obj)
        {
            if (obj.Topic == EventTopicNames.EditTicketNote)
            {
                _ticketNoteEditorViewModel.SelectedTicket = obj.Value;
                DisplayTicketNoteEditor();
            }
        }

        private void OnDisplayTicketDetailsScreen(EventParameters<EventAggregator> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayTicketOrderDetails)
                DisplayTicketDetailsScreen();
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(OrderTagGroupEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(TicketNoteEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(TicketTagEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(OrderStateEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(OrderTagEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(ProductTimerEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof (AutomationCommandSelectorView));
        }

        public void DisplayTicketDetailsScreen()
        {
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_selectedOrdersView);
            _ticketNoteEditorView.TicketNote.BackgroundFocus();
        }

        public void DisplayAutomationCommandValueSelector()
        {
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_automationCommandSelectorView);
        }

        public void DisplayTicketNoteEditor()
        {
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_ticketNoteEditorView);
        }

        public void DisplayTicketTagEditor()
        {
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_ticketTagEditorView);
        }

        private void DisplayOrderStateEditor()
        {
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_orderStateEditorView);
        }

        private void DisplayOrderTagEditor()
        {
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_orderTagEditorView);
        }

        private void DisplayProdcutTimerEdior(Order selectedOrder)
        {
            _productTimerEditorViewModel.Update(selectedOrder);
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_productTimerEditorView);
        }

        private void OnSelectedOrdersDataEvent(EventParameters<SelectedOrdersData> selectedOrdersEvent)
        {
            if (selectedOrdersEvent.Topic == EventTopicNames.SelectedOrdersChanged)
            {
                _selectedOrders = selectedOrdersEvent.Value.SelectedOrders.ToList();

                if (_selectedOrdersViewModel.ShouldDisplay(selectedOrdersEvent.Value.Ticket, _selectedOrders))
                {
                    DisplayTicketDetailsScreen();
                }
                else if (_productTimerEditorViewModel.ShouldDisplay(selectedOrdersEvent.Value.Ticket, _selectedOrders.ToList()))
                {
                    DisplayProdcutTimerEdior(_selectedOrders.First());
                }
            }
        }
    }
}
