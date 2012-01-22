using System;
using System.ComponentModel.Composition;
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
        private readonly SelectedOrdersView _selectedOrdersView;
        private readonly SelectedOrdersViewModel _selectedOrdersViewModel;
        private readonly IRegionManager _regionManager;
        private readonly IUserService _userService;

        private readonly ICaptionCommand _showExtraModifierCommand;

        [ImportingConstructor]
        public ModifierModule(IRegionManager regionManager, IUserService userService,
            SelectedOrdersView selectedOrdersView, SelectedOrdersViewModel selectedOrdersViewModel)
        {
            _selectedOrdersView = selectedOrdersView;
            _selectedOrdersViewModel = selectedOrdersViewModel;
            _regionManager = regionManager;
            _userService = userService;

            EventServiceFactory.EventService.GetEvent<GenericEvent<SelectedOrdersData>>().Subscribe(OnSelectedOrdersDataEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnDisplayTicketDetailsScreen);

            _showExtraModifierCommand = new CaptionCommand<Ticket>(Resources.ExtraModifier, OnExtraModifiersSelected, CanSelectExtraModifier);
            _showExtraModifierCommand.PublishEvent(EventTopicNames.AddCustomOrderCommand);

        }

        private bool CanSelectExtraModifier(Ticket arg)
        {
            return _selectedOrdersViewModel.SelectedOrder != null && !_selectedOrdersViewModel.SelectedOrder.Locked &&
                   _userService.IsUserPermittedFor(PermissionNames.ChangeExtraProperty);
        }

        private void OnExtraModifiersSelected(Ticket obj)
        {
            DisplayTicketDetailsScreen();
            _selectedOrdersViewModel.ActivateExtraModifierScreen(obj);
        }

        private void OnDisplayTicketDetailsScreen(EventParameters<EventAggregator> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayTicketOrderDetails)
                DisplayTicketDetailsScreen();
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.PosSubRegion, typeof(SelectedOrdersView));
        }

        public void DisplayTicketDetailsScreen()
        {
            _regionManager.Regions[RegionNames.PosSubRegion].Activate(_selectedOrdersView);
        }

        private void OnSelectedOrdersDataEvent(EventParameters<SelectedOrdersData> selectedOrdersEvent)
        {
            if (selectedOrdersEvent.Topic == EventTopicNames.SelectedOrdersChanged)
            {
                if (_selectedOrdersViewModel.ShouldDisplay(selectedOrdersEvent.Value.Ticket, selectedOrdersEvent.Value.SelectedOrders))
                    DisplayTicketDetailsScreen();
                //else EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
            }
        }
    }
}
