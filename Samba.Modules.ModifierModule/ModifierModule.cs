using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
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
        private readonly AutomationCommandSelectorViewModel _automationCommandSelectorViewModel;
        private readonly AutomationCommandValueSelectorView _automationCommandValueSelectorView;
        private readonly ProductTimerEditorView _productTimerEditorView;
        private readonly ProductTimerEditorViewModel _productTimerEditorViewModel;
        private readonly TicketLogViewerView _ticketLogViewerView;
        private readonly TicketLogViewerViewModel _ticketLogViewerViewModel;
        private readonly IRegionManager _regionManager;

        private readonly TicketNoteEditorView _ticketNoteEditorView;
        private readonly TicketNoteEditorViewModel _ticketNoteEditorViewModel;
        private readonly TicketTagEditorView _ticketTagEditorView;
        private readonly TicketTagEditorViewModel _ticketTagEditorViewModel;

        [ImportingConstructor]
        public ModifierModule(IRegionManager regionManager, IUserService userService,
            TicketNoteEditorView ticketNoteEditorView, TicketNoteEditorViewModel ticketNoteEditorViewModel,
            TicketTagEditorView ticketTagEditorView, TicketTagEditorViewModel ticketTagEditorViewModel,
            OrderTagGroupEditorView selectedOrdersView, OrderTagGroupEditorViewModel selectedOrdersViewModel,
            AutomationCommandSelectorView automationCommandSelectorView, AutomationCommandSelectorViewModel automationCommandSelectorViewModel,
            AutomationCommandValueSelectorView automationCommandValueSelectorView, AutomationCommandValueSelectorViewModel automationCommandValueSelectorViewModel,
            ProductTimerEditorView productTimerEditorView, ProductTimerEditorViewModel productTimerEditorViewModel,
            TicketLogViewerView ticketLogViewerView, TicketLogViewerViewModel ticketLogViewerViewModel)
        {
            _selectedOrdersView = selectedOrdersView;
            _selectedOrdersViewModel = selectedOrdersViewModel;
            _automationCommandSelectorView = automationCommandSelectorView;
            _automationCommandSelectorViewModel = automationCommandSelectorViewModel;
            _automationCommandValueSelectorView = automationCommandValueSelectorView;
            _productTimerEditorView = productTimerEditorView;
            _productTimerEditorViewModel = productTimerEditorViewModel;
            _ticketLogViewerView = ticketLogViewerView;
            _ticketLogViewerViewModel = ticketLogViewerViewModel;
            _ticketNoteEditorView = ticketNoteEditorView;
            _ticketNoteEditorViewModel = ticketNoteEditorViewModel;
            _ticketTagEditorView = ticketTagEditorView;
            _ticketTagEditorViewModel = ticketTagEditorViewModel;

            _regionManager = regionManager;

            EventServiceFactory.EventService.GetEvent<GenericEvent<OperationRequest<SelectedOrdersData>>>().Subscribe(OnSelectedOrdersDataEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagData>>().Subscribe(OnTicketTagDataSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnTicketEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<AutomationCommand>>().Subscribe(OnAutomationCommandEvent);
        }

        private void OnAutomationCommandEvent(EventParameters<AutomationCommand> obj)
        {
            if (obj.Topic == EventTopicNames.SelectAutomationCommandValue)
            {
                DisplayAutomationCommandValueSelector();
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
            if (obj.Topic == EventTopicNames.SelectAutomationCommand)
            {
                _automationCommandSelectorViewModel.SelectedTicket = obj.Value;
                DisplayAutomationCommandSelector();
            }

            if (obj.Topic == EventTopicNames.EditTicketNote)
            {
                _ticketNoteEditorViewModel.SelectedTicket = obj.Value;
                DisplayTicketNoteEditor();
            }

            if (obj.Topic == EventTopicNames.DisplayTicketLog)
            {
                _ticketLogViewerViewModel.SelectedTicket = obj.Value;
                DisplayTicketLogViewer();
            }
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(OrderTagGroupEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(TicketNoteEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(TicketTagEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(ProductTimerEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(AutomationCommandSelectorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(AutomationCommandValueSelectorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(TicketLogViewerView));
        }

        public void DisplayTicketDetailsScreen(OperationRequest<SelectedOrdersData> currentOperationRequest)
        {
            _selectedOrdersViewModel.CurrentOperationRequest = currentOperationRequest;
            _regionManager.ActivateRegion(RegionNames.PosSubRegion, _selectedOrdersView);
            _ticketNoteEditorView.TicketNote.BackgroundFocus();
        }

        public void DisplayAutomationCommandSelector()
        {
            _regionManager.ActivateRegion(RegionNames.PosSubRegion, _automationCommandSelectorView);
        }

        public void DisplayAutomationCommandValueSelector()
        {
            _regionManager.ActivateRegion(RegionNames.PosSubRegion, _automationCommandValueSelectorView);
        }

        public void DisplayTicketNoteEditor()
        {
            _regionManager.ActivateRegion(RegionNames.PosSubRegion, _ticketNoteEditorView);
        }

        public void DisplayTicketLogViewer()
        {
            _regionManager.ActivateRegion(RegionNames.PosSubRegion, _ticketLogViewerView);
        }

        public void DisplayTicketTagEditor()
        {
            _regionManager.ActivateRegion(RegionNames.PosSubRegion, _ticketTagEditorView);
        }

        private void DisplayProdcutTimerEdior(Order selectedOrder)
        {
            _productTimerEditorViewModel.Update(selectedOrder);
            _regionManager.ActivateRegion(RegionNames.PosSubRegion, _productTimerEditorView);
        }

        private void OnSelectedOrdersDataEvent(EventParameters<OperationRequest<SelectedOrdersData>> selectedOrdersEvent)
        {
            if (selectedOrdersEvent.Topic == EventTopicNames.DisplayTicketOrderDetails)
            {
                _selectedOrders = selectedOrdersEvent.Value.SelectedItem.SelectedOrders.ToList();

                if (_selectedOrdersViewModel.ShouldDisplay(selectedOrdersEvent.Value.SelectedItem.Ticket, _selectedOrders))
                {
                    DisplayTicketDetailsScreen(selectedOrdersEvent.Value);
                }
                else if (_productTimerEditorViewModel.ShouldDisplay(selectedOrdersEvent.Value.SelectedItem.Ticket, _selectedOrders.ToList()))
                {
                    DisplayProdcutTimerEdior(_selectedOrders.First());
                }
                else EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
            }
        }
    }
}
