﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ModifierModule
{
    [Export]
    public class OrderTagGroupEditorViewModel : ObservableObject
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public OrderTagGroupEditorViewModel(IUserService userService, IApplicationState applicationState, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            ToggleRemoveModeCommand = new CaptionCommand<string>(Resources.Remove, OnToggleRemoveMode);
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            PortionSelectedCommand = new DelegateCommand<MenuItemPortion>(OnPortionSelected);
            GroupedOrderTagSelectedCommand = new DelegateCommand<GroupedOrderTagButtonViewModel>(OnGroupedOrderTagSelected);
            OrderTagSelectedCommand = new DelegateCommand<OrderTagButtonViewModel>(OnOrderTagSelected);
            FreeTagSelectedCommand = new DelegateCommand<OrderTagGroupViewModel>(OnFreeTagSelected);
            SelectedItemPortions = new ObservableCollection<MenuItemPortion>();
            OrderTagGroups = new ObservableCollection<OrderTagGroupViewModel>();
            GroupedOrderTagGroups = new ObservableCollection<GroupedOrderTagViewModel>();
        }



        private void ResetValues(Ticket selectedTicket)
        {
            SelectedTicket = null;
            SelectedOrder = null;

            SelectedItemPortions.Clear();
            OrderTagGroups.Clear();
            GroupedOrderTagGroups.Clear();

            SetSelectedTicket(selectedTicket);
        }

        public Ticket SelectedTicket { get; private set; }
        public Order SelectedOrder { get; private set; }

        private bool _removeMode;
        public bool RemoveMode
        {
            get { return _removeMode; }
            set
            {
                _removeMode = value;
                RaisePropertyChanged(() => RemoveMode);
                RaisePropertyChanged(() => RemoveModeButtonColor);
            }
        }

        public string RemoveModeButtonColor { get { return RemoveMode ? "Black" : "Gainsboro"; } }

        public ICaptionCommand CloseCommand { get; set; }
        public CaptionCommand<string> ToggleRemoveModeCommand { get; set; }
        public DelegateCommand<OrderTagGroupViewModel> FreeTagSelectedCommand { get; set; }

        public DelegateCommand<MenuItemPortion> PortionSelectedCommand { get; set; }
        public ObservableCollection<MenuItemPortion> SelectedItemPortions { get; set; }

        public DelegateCommand<OrderTagButtonViewModel> OrderTagSelectedCommand { get; set; }
        public DelegateCommand<GroupedOrderTagButtonViewModel> GroupedOrderTagSelectedCommand { get; set; }
        public ObservableCollection<OrderTagGroupViewModel> OrderTagGroups { get; set; }
        public ObservableCollection<GroupedOrderTagViewModel> GroupedOrderTagGroups { get; set; }

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

        private void OnToggleRemoveMode(string obj)
        {
            RemoveMode = !RemoveMode;
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

            orderTagData.PublishEvent(RemoveMode ? EventTopicNames.OrderTagRemoved : EventTopicNames.OrderTagSelected, true);
            RemoveMode = false;
            OrderTagGroups.Where(x => x.OrderTags.Contains(orderTag)).ToList().ForEach(x => x.Refresh());
        }

        private void OnGroupedOrderTagSelected(GroupedOrderTagButtonViewModel obj)
        {
            if (!RemoveMode) obj.UpdateNextTag(obj.NextTag);
            var orderTagData = new OrderTagData
                                   {
                                       OrderTagGroup = obj.OrderTagGroup,
                                       SelectedOrderTag = obj.CurrentTag,
                                       Ticket = SelectedTicket
                                   };
            if (RemoveMode) obj.UpdateNextTag(null);
            orderTagData.PublishEvent(RemoveMode ? EventTopicNames.OrderTagRemoved : EventTopicNames.OrderTagSelected, true);
            RemoveMode = false;
        }


        private void OnFreeTagSelected(OrderTagGroupViewModel obj)
        {
            if (string.IsNullOrEmpty(obj.FreeTagName) || string.IsNullOrEmpty(obj.FreeTagName.Trim())) return;
            if (obj.OrderTags.Any(x => x.Name.ToLower() == obj.FreeTagName.ToLower()))
            {
                var b = obj.OrderTags.First(x => x.Name == obj.FreeTagName.ToLower());
                OnOrderTagSelected(b);
                return;
            }

            var orderTagData = new OrderTagData
            {
                OrderTagGroup = obj.Model,
                SelectedOrderTag = new OrderTag { Name = obj.FreeTagName, Price = obj.FreeTagPrice },
                Ticket = SelectedTicket
            };

            obj.FreeTagName = "";
            obj.FreeTagPriceStr = "0";
            obj.CreateOrderTagButton(orderTagData);
            orderTagData.PublishEvent(RemoveMode ? EventTopicNames.OrderTagRemoved : EventTopicNames.OrderTagSelected, true);
            RemoveMode = false;
            OrderTagGroups.Where(x => x.OrderTags.Any(y => y.Name == obj.FreeTagName)).ToList().ForEach(x => x.Refresh());
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

                RaisePropertyChanged(() => IsPortionsVisible);

                var orderTagGroups = _applicationState.GetOrderTagGroups(SelectedOrder.MenuItemId).ToList();

                OrderTagGroups.AddRange(
                    orderTagGroups
                    .Where(x => string.IsNullOrEmpty(x.GroupTag))
                    .Select(x => new OrderTagGroupViewModel(x, so)));

                if (SelectedOrder != null)
                {
                    GroupedOrderTagGroups.AddRange(
                        orderTagGroups
                        .Where(x => !string.IsNullOrEmpty(x.GroupTag) && x.OrderTags.Count > 1)
                        .GroupBy(x => x.GroupTag)
                        .Select(x => new GroupedOrderTagViewModel(SelectedOrder, x)));
                }
            }

            return SelectedItemPortions.Count > 1 || OrderTagGroups.Count > 0 || GroupedOrderTagGroups.Count > 0;
        }

    }
}
