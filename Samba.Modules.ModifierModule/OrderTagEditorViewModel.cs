using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ModifierModule
{
    [Export]
    public class OrderTagEditorViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        
        [ImportingConstructor]
        public OrderTagEditorViewModel(IUserService userService, ICacheService cacheService)
        {
            _cacheService = cacheService;
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            PortionSelectedCommand = new DelegateCommand<MenuItemPortion>(OnPortionSelected);
            OrderTagSelectedCommand = new DelegateCommand<OrderTagButtonViewModel>(OnOrderTagSelected);
            SelectedItemPortions = new ObservableCollection<MenuItemPortion>();
            OrderTagGroups = new ObservableCollection<SelectedOrderTagGroupViewModel>();
            OrderTags = new ObservableCollection<OrderTagButtonViewModel>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagDataSelected);
        }

        private void OnOrderTagDataSelected(EventParameters<OrderTagData> obj)
        {
            if (obj.Topic == EventTopicNames.SelectOrderTag)
            {
                ResetValues(obj.Value.Ticket);
                SelectedOrderTagData = obj.Value;
                OrderTags.AddRange(obj.Value.OrderTagGroup.OrderTags.Select(x => new OrderTagButtonViewModel(obj.Value.SelectedOrders, obj.Value.OrderTagGroup, x)));
                if (OrderTags.Count == 1)
                {
                    obj.Value.SelectedOrderTag = OrderTags[0].Model;
                    obj.Value.PublishEvent(EventTopicNames.OrderTagSelected);
                    return;
                }
                RaisePropertyChanged(() => OrderTagColumnCount);
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.DisplayTicketOrderDetails);
            }
        }

        private void ResetValues(Ticket selectedTicket)
        {
            SelectedTicket = null;
            SelectedOrder = null;
            SelectedOrderTagData = null;

            SelectedItemPortions.Clear();
            OrderTagGroups.Clear();
            OrderTags.Clear();
            
            SetSelectedTicket(selectedTicket);
        }

        public Ticket SelectedTicket { get; private set; }
        public Order SelectedOrder { get; private set; }
        public OrderTagData SelectedOrderTagData { get; set; }

        public ICaptionCommand CloseCommand { get; set; }

        public DelegateCommand<MenuItemPortion> PortionSelectedCommand { get; set; }
        public ObservableCollection<MenuItemPortion> SelectedItemPortions { get; set; }

        public DelegateCommand<OrderTagButtonViewModel> OrderTagSelectedCommand { get; set; }
        public ObservableCollection<SelectedOrderTagGroupViewModel> OrderTagGroups { get; set; }
        
        public ObservableCollection<OrderTagButtonViewModel> OrderTags { get; set; }
        public int OrderTagColumnCount { get { return OrderTags.Count % 7 == 0 ? OrderTags.Count / 7 : (OrderTags.Count / 7) + 1; } }

        public bool IsPortionsVisible
        {
            get
            {
                return SelectedOrder != null
                    && SelectedOrder.DecreaseInventory
                    && !SelectedOrder.Locked
                    && SelectedItemPortions.Count > 0;
            }
        }

        private static void OnCloseCommandExecuted(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnPortionSelected(MenuItemPortion obj)
        {
            obj.PublishEvent(EventTopicNames.PortionSelected);
            if (OrderTagGroups.Count == 0)
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnOrderTagSelected(OrderTagButtonViewModel orderTag)
        {
            var mig = SelectedOrderTagData != null
                ? SelectedOrderTagData.OrderTagGroup
                : OrderTagGroups.FirstOrDefault(propertyGroup => propertyGroup.OrderTags.Contains(orderTag)).Model;
            Debug.Assert(mig != null);

            var orderTagData = new OrderTagData
                                   {
                                       OrderTagGroup = mig,
                                       SelectedOrderTag = orderTag.Model,
                                       Ticket = SelectedTicket
                                   };

            orderTagData.PublishEvent(EventTopicNames.OrderTagSelected, true);

            OrderTags.ToList().ForEach(x => x.Refresh());
            OrderTagGroups.Where(x => x.OrderTags.Contains(orderTag)).ToList().ForEach(x => x.Refresh());
        }

        private void SetSelectedTicket(Ticket ticketViewModel)
        {
            SelectedTicket = ticketViewModel;
            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => SelectedOrder);
            RaisePropertyChanged(() => IsPortionsVisible);
        }

        public bool ShouldDisplay(Ticket value, IEnumerable<Order> selectedOrders)
        {
            ResetValues(value);
            SelectedOrder = selectedOrders.Count() == 1 ? selectedOrders.ElementAt(0) : null;
            if (selectedOrders.Any(x => x.Locked || !x.DecreaseInventory)) return false;

            if (SelectedTicket != null && SelectedOrder != null)
            {
                var portions = _cacheService.GetMenuItemPortions(SelectedOrder.MenuItemId);

                if (SelectedOrder.PortionCount > 1)
                {
                    SelectedItemPortions.AddRange(portions);
                }

                OrderTagGroups.AddRange(
                    _cacheService.GetOrderTagGroupsForItem(SelectedOrder.MenuItemId)
                    .Where(x => string.IsNullOrEmpty(x.ButtonHeader))
                    .Select(x => new SelectedOrderTagGroupViewModel(x, selectedOrders)));

                RaisePropertyChanged(() => IsPortionsVisible);
            }

            return SelectedItemPortions.Count > 1 || OrderTagGroups.Count > 0;
        }

    }
}
