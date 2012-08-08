using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
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

        private readonly OrderTagEditorView _selectedOrdersView;
        private readonly OrderTagEditorViewModel _selectedOrdersViewModel;
        private readonly IRegionManager _regionManager;
        private readonly IUserService _userService;

        private readonly ICaptionCommand _showExtraModifierCommand;
        private readonly TicketNoteEditorView _ticketNoteEditorView;
        private readonly TicketNoteEditorViewModel _ticketNoteEditorViewModel;
        private readonly TicketTagEditorView _ticketTagEditorView;
        private readonly TicketTagEditorViewModel _ticketTagEditorViewModel;
        private readonly OrderStateEditorView _orderStateEditorView;
        private readonly OrderStateEditorViewModel _orderStateEditorViewModel;
        private readonly ExtraModifierEditorViewModel _extraModifierEditorViewModel;
        private readonly ExtraModifierEditorView _extraModifierEditorView;

        [ImportingConstructor]
        public ModifierModule(IRegionManager regionManager, IUserService userService,
            ExtraModifierEditorView extraModifierEditorView, ExtraModifierEditorViewModel extraModifierEditorViewModel,
            TicketNoteEditorView ticketNoteEditorView, TicketNoteEditorViewModel ticketNoteEditorViewModel,
            TicketTagEditorView ticketTagEditorView, TicketTagEditorViewModel ticketTagEditorViewModel,
            OrderStateEditorView orderStateEditorView,OrderStateEditorViewModel orderStateEditorViewModel,
            OrderTagEditorView selectedOrdersView, OrderTagEditorViewModel selectedOrdersViewModel)
        {
            _selectedOrdersView = selectedOrdersView;
            _selectedOrdersViewModel = selectedOrdersViewModel;
            _ticketNoteEditorView = ticketNoteEditorView;
            _ticketNoteEditorViewModel = ticketNoteEditorViewModel;
            _ticketTagEditorView = ticketTagEditorView;
            _ticketTagEditorViewModel = ticketTagEditorViewModel;
            _orderStateEditorView = orderStateEditorView;
            _orderStateEditorViewModel = orderStateEditorViewModel;
            _extraModifierEditorViewModel = extraModifierEditorViewModel;
            _extraModifierEditorView = extraModifierEditorView;

            _regionManager = regionManager;
            _userService = userService;

            EventServiceFactory.EventService.GetEvent<GenericEvent<SelectedOrdersData>>().Subscribe(OnSelectedOrdersDataEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnDisplayTicketDetailsScreen);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagData>>().Subscribe(OnTicketTagDataSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderStateData>>().Subscribe(OnOrderStateDataSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnTicketEvent);

            _showExtraModifierCommand = new CaptionCommand<Ticket>(Resources.ExtraModifier, OnExtraModifiersSelected, CanSelectExtraModifier);
            _showExtraModifierCommand.PublishEvent(EventTopicNames.AddCustomOrderCommand);
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

        private bool CanSelectExtraModifier(Ticket arg)
        {
            return _selectedOrdersViewModel.SelectedOrder != null && !_selectedOrdersViewModel.SelectedOrder.Locked &&
                   _userService.IsUserPermittedFor(PermissionNames.ChangeExtraProperty);
        }

        private void OnExtraModifiersSelected(Ticket obj)
        {
            _extraModifierEditorViewModel.SelectedOrder = _selectedOrders.Count() == 1
                ? _selectedOrders.ElementAt(0) : Order.Null;
            DisplayExtraModifierEditor();
        }

        private void OnDisplayTicketDetailsScreen(EventParameters<EventAggregator> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayTicketOrderDetails)
                DisplayTicketDetailsScreen();
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(OrderTagEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(TicketNoteEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(TicketTagEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion,typeof (OrderStateEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(ExtraModifierEditorView));
        }

        public void DisplayExtraModifierEditor()
        {
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_extraModifierEditorView);
        }

        public void DisplayTicketDetailsScreen()
        {
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_selectedOrdersView);
            _ticketNoteEditorView.TicketNote.BackgroundFocus();
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

        private void OnSelectedOrdersDataEvent(EventParameters<SelectedOrdersData> selectedOrdersEvent)
        {
            if (selectedOrdersEvent.Topic == EventTopicNames.SelectedOrdersChanged)
            {
                _selectedOrders = selectedOrdersEvent.Value.SelectedOrders;

                if (_selectedOrdersViewModel.ShouldDisplay(selectedOrdersEvent.Value.Ticket, _selectedOrders))
                    DisplayTicketDetailsScreen();
            }
        }
    }
}
