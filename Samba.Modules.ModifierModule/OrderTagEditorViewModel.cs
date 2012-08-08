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
        }

        private void ResetValues(Ticket selectedTicket)
        {
            SelectedTicket = null;
            SelectedOrder = null;

            SelectedItemPortions.Clear();
            OrderTagGroups.Clear();

            SetSelectedTicket(selectedTicket);
        }

        public Ticket SelectedTicket { get; private set; }
        public Order SelectedOrder { get; private set; }

        public ICaptionCommand CloseCommand { get; set; }

        public DelegateCommand<MenuItemPortion> PortionSelectedCommand { get; set; }
        public ObservableCollection<MenuItemPortion> SelectedItemPortions { get; set; }

        public DelegateCommand<OrderTagButtonViewModel> OrderTagSelectedCommand { get; set; }
        public ObservableCollection<SelectedOrderTagGroupViewModel> OrderTagGroups { get; set; }

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
            var mig = OrderTagGroups.First(propertyGroup => propertyGroup.OrderTags.Contains(orderTag)).Model;
            Debug.Assert(mig != null);

            var orderTagData = new OrderTagData
                                   {
                                       OrderTagGroup = mig,
                                       SelectedOrderTag = orderTag.Model,
                                       Ticket = SelectedTicket
                                   };

            orderTagData.PublishEvent(EventTopicNames.OrderTagSelected, true);
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
            if (selectedOrders == null) return false;
            var so = selectedOrders.ToList();
            SelectedOrder = so.Count() == 1 ? so.ElementAt(0) : null;
            if (so.Any(x => x.Locked || !x.DecreaseInventory)) return false;

            if (SelectedTicket != null && SelectedOrder != null)
            {
                var portions = _cacheService.GetMenuItemPortions(SelectedOrder.MenuItemId);

                if (SelectedOrder.PortionCount > 1)
                {
                    SelectedItemPortions.AddRange(portions);
                }

                OrderTagGroups.AddRange(
                    _cacheService.GetOrderTagGroupsForItem(SelectedOrder.MenuItemId)
                    .Select(x => new SelectedOrderTagGroupViewModel(x, so)));

                RaisePropertyChanged(() => IsPortionsVisible);
            }

            return SelectedItemPortions.Count > 1 || OrderTagGroups.Count > 0;
        }

    }
}
