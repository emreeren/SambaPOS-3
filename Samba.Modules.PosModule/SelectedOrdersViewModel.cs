using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.UIControls;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class SelectedOrdersViewModel : ObservableObject
    {
        private bool _showExtraPropertyEditor;
        private bool _showTicketNoteEditor;
        private bool _showFreeTagEditor;
        private readonly IApplicationState _applicationState;
        private readonly ITicketService _ticketService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public SelectedOrdersViewModel(IApplicationState applicationState, ITicketService ticketService,
            IUserService userService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _ticketService = ticketService;
            _cacheService = cacheService;
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            SelectTicketTagCommand = new DelegateCommand<TicketTag>(OnTicketTagSelected);
            PortionSelectedCommand = new DelegateCommand<MenuItemPortion>(OnPortionSelected);
            OrderTagSelectedCommand = new DelegateCommand<OrderTagButtonViewModel>(OnOrderTagSelected);
            UpdateExtraPropertiesCommand = new CaptionCommand<string>(Resources.Update, OnUpdateExtraProperties);
            UpdateFreeTagCommand = new CaptionCommand<string>(Resources.AddAndSave, OnUpdateFreeTag, CanUpdateFreeTag);
            SelectedItemPortions = new ObservableCollection<MenuItemPortion>();
            OrderTagGroups = new ObservableCollection<SelectedOrderTagGroupViewModel>();
            TicketTags = new ObservableCollection<TicketTag>();
            OrderTags = new ObservableCollection<OrderTagButtonViewModel>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagDataSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagData>>().Subscribe(OnTicketTagDataSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnTicketEvent);
        }

        private void OnTicketTagDataSelected(EventParameters<TicketTagData> obj)
        {
            if (obj.Topic == EventTopicNames.SelectTicketTag)
            {
                ResetValues(obj.Value.Ticket);
                _showFreeTagEditor = obj.Value.TicketTagGroup.FreeTagging;
                SelectedTicketTagData = obj.Value;
                List<TicketTag> ticketTags;
                if (_showFreeTagEditor)
                {
                    ticketTags = _cacheService.GetTicketTagGroupById(obj.Value.TicketTagGroup.Id)
                        .TicketTags.OrderBy(x => x.Name).ToList();
                }
                else
                {
                    ticketTags = _applicationState.CurrentDepartment.TicketTemplate.TicketTagGroups.Where(
                           x => x.Name == obj.Value.TicketTagGroup.Name).SelectMany(x => x.TicketTags).ToList();
                }
                ticketTags.Sort(new AlphanumComparator());
                TicketTags.AddRange(ticketTags);

                if (SelectedTicket.IsTaggedWith(obj.Value.TicketTagGroup.Name)) TicketTags.Add(TicketTag.Empty);
                if (TicketTags.Count == 1 && !_showFreeTagEditor)
                {
                    obj.Value.SelectedTicketTag = TicketTags[0];
                    obj.Value.PublishEvent(EventTopicNames.TicketTagSelected);
                    return;
                }

                RaisePropertyChanged(() => TagColumnCount);
                RaisePropertyChanged(() => IsFreeTagEditorVisible);
                RaisePropertyChanged(() => FilteredTextBoxType);
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.DisplayTicketOrderDetails);
            }
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

        private void OnTicketEvent(EventParameters<Ticket> obj)
        {
            if (obj.Topic == EventTopicNames.SelectExtraProperty)
            {
                ResetValues(obj.Value);
                _showExtraPropertyEditor = true;
                RaisePropertyChanged(() => IsExtraPropertyEditorVisible);
                RaisePropertyChanged(() => IsPortionsVisible);
            }

            if (obj.Topic == EventTopicNames.EditTicketNote)
            {
                ResetValues(obj.Value);
                _showTicketNoteEditor = true;
                RaisePropertyChanged(() => IsTicketNoteEditorVisible);
            }
        }

        private void ResetValues(Ticket selectedTicket)
        {

            SelectedTicket = null;
            SelectedOrder = null;
            SelectedOrderTagData = null;
            SelectedTicketTagData = null;

            SelectedItemPortions.Clear();
            OrderTagGroups.Clear();
            TicketTags.Clear();
            OrderTags.Clear();
            _showExtraPropertyEditor = false;
            _showTicketNoteEditor = false;
            _showFreeTagEditor = false;
            SetSelectedTicket(selectedTicket);
        }

        public Ticket SelectedTicket { get; private set; }
        public Order SelectedOrder { get; private set; }
        public OrderTagData SelectedOrderTagData { get; set; }
        public TicketTagData SelectedTicketTagData { get; set; }

        public ICaptionCommand CloseCommand { get; set; }
        public ICaptionCommand UpdateExtraPropertiesCommand { get; set; }
        public ICaptionCommand UpdateFreeTagCommand { get; set; }
        public ICommand SelectTicketTagCommand { get; set; }

        public DelegateCommand<MenuItemPortion> PortionSelectedCommand { get; set; }
        public ObservableCollection<MenuItemPortion> SelectedItemPortions { get; set; }

        public DelegateCommand<OrderTagButtonViewModel> OrderTagSelectedCommand { get; set; }
        public ObservableCollection<SelectedOrderTagGroupViewModel> OrderTagGroups { get; set; }

        public ObservableCollection<TicketTag> TicketTags { get; set; }
        public ObservableCollection<OrderTagButtonViewModel> OrderTags { get; set; }

        public int TagColumnCount { get { return TicketTags.Count % 7 == 0 ? TicketTags.Count / 7 : (TicketTags.Count / 7) + 1; } }
        public int OrderTagColumnCount { get { return OrderTags.Count % 7 == 0 ? OrderTags.Count / 7 : (OrderTags.Count / 7) + 1; } }

        public FilteredTextBox.FilteredTextBoxType FilteredTextBoxType
        {
            get
            {
                if (SelectedTicketTagData != null && SelectedTicketTagData.TicketTagGroup != null)
                {
                    if (SelectedTicketTagData.TicketTagGroup.IsInteger)
                        return FilteredTextBox.FilteredTextBoxType.Digits;
                    if (SelectedTicketTagData.TicketTagGroup.IsDecimal)
                        return FilteredTextBox.FilteredTextBoxType.Number;
                }
                return FilteredTextBox.FilteredTextBoxType.Letters;
            }
        }

        private string _freeTag;
        public string FreeTag
        {
            get { return _freeTag; }
            set
            {
                _freeTag = value;
                RaisePropertyChanged(() => FreeTag);
            }
        }

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

        private void OnCloseCommandExecuted(string obj)
        {
            _showTicketNoteEditor = false;
            _showExtraPropertyEditor = false;
            _showFreeTagEditor = false;
            FreeTag = string.Empty;
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        public bool IsFreeTagEditorVisible { get { return _showFreeTagEditor; } }
        public bool IsExtraPropertyEditorVisible { get { return _showExtraPropertyEditor && SelectedOrder != null; } }
        public bool IsTicketNoteEditorVisible { get { return _showTicketNoteEditor; } }


        private bool CanUpdateFreeTag(string arg)
        {
            return !string.IsNullOrEmpty(FreeTag);
        }

        private void OnUpdateFreeTag(string obj)
        {
            SelectedTicketTagData.SelectedTicketTag = new TicketTag { Name = FreeTag };
            SelectedTicketTagData.PublishEvent(EventTopicNames.TicketTagSelected);
            FreeTag = string.Empty;
        }

        private void OnUpdateExtraProperties(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
            _showExtraPropertyEditor = false;
            RaisePropertyChanged(() => IsExtraPropertyEditorVisible);
        }

        private void OnTicketTagSelected(TicketTag obj)
        {
            SelectedTicketTagData.SelectedTicketTag = obj;
            SelectedTicketTagData.PublishEvent(EventTopicNames.TicketTagSelected);
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

            orderTagData.PublishEvent(EventTopicNames.OrderTagSelected);
        }

        private void SetSelectedTicket(Ticket ticketViewModel)
        {
            SelectedTicket = ticketViewModel;
            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => SelectedOrder);
            RaisePropertyChanged(() => IsTicketNoteEditorVisible);
            RaisePropertyChanged(() => IsExtraPropertyEditorVisible);
            RaisePropertyChanged(() => IsFreeTagEditorVisible);
            RaisePropertyChanged(() => IsPortionsVisible);
        }

        public bool ShouldDisplay(Ticket value, IEnumerable<Order> selectedOrders)
        {
            if (selectedOrders.Any(x => x.Locked)) return false;
            ResetValues(value);

            SelectedOrder = selectedOrders.Count() == 1 ? selectedOrders.ElementAt(0) : null;

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
