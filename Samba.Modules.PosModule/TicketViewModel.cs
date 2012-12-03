using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Controls.Interaction;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IExpressionService _expressionService;
        private readonly IAutomationService _automationService;
        private readonly TicketOrdersViewModel _ticketOrdersViewModel;
        private readonly TicketTotalsViewModel _totals;

        public CaptionCommand<string> PrintTicketCommand { get; set; }
        public CaptionCommand<string> PrintInvoiceCommand { get; set; }
        public CaptionCommand<string> MoveOrdersCommand { get; set; }

        public ICaptionCommand IncQuantityCommand { get; set; }
        public ICaptionCommand DecQuantityCommand { get; set; }
        public ICaptionCommand IncSelectionQuantityCommand { get; set; }
        public ICaptionCommand DecSelectionQuantityCommand { get; set; }
        public ICaptionCommand ShowTicketTagsCommand { get; set; }
        public ICaptionCommand ShowOrderStatesCommand { get; set; }
        public ICaptionCommand CancelItemCommand { get; set; }
        public ICaptionCommand EditTicketNoteCommand { get; set; }
        public ICaptionCommand RemoveTicketLockCommand { get; set; }
        public ICaptionCommand RemoveTicketTagCommand { get; set; }
        public ICaptionCommand ChangePriceCommand { get; set; }

        public DelegateCommand<ResourceType> SelectResourceCommand { get; set; }
        public DelegateCommand<CommandContainerButton> ExecuteAutomationCommnand { get; set; }

        private ObservableCollection<ResourceButton> _resourceButtons;
        public ObservableCollection<ResourceButton> ResourceButtons
        {
            get
            {
                if (_resourceButtons == null && SelectedDepartment != null)
                {
                    _resourceButtons = new ObservableCollection<ResourceButton>(
                        _applicationState.GetTicketResourceScreens()
                        .OrderBy(x => x.Order)
                        .Select(x => _cacheService.GetResourceTypeById(x.ResourceTypeId))
                        .Distinct()
                        .Select(x => new ResourceButton(x, SelectedTicket)));
                }
                return _resourceButtons;
            }
        }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _resourceButtons = null;
                _allAutomationCommands = null;
                _selectedTicket = value ?? Ticket.Empty;
                _totals.Model = _selectedTicket;
                _ticketOrdersViewModel.SelectedTicket = _selectedTicket;
                _ticketInfo.SelectedTicket = _selectedTicket;
                _paymentButtonViewModel.SelectedTicket = _selectedTicket;
                RaisePropertyChanged(() => ResourceButtons);
                RaisePropertyChanged(() => TicketAutomationCommands);
            }
        }

        private readonly PaymentButtonViewModel _paymentButtonViewModel;

        private readonly TicketInfoViewModel _ticketInfo;
        public TicketInfoViewModel TicketInfo { get { return _ticketInfo; } }

        public IEnumerable<Order> SelectedOrders { get { return SelectedTicket.SelectedOrders; } }

        public Order SelectedOrder
        {
            get
            {
                return _ticketOrdersViewModel != null && SelectedOrders.Count() == 1 ? SelectedOrders.ElementAt(0) : null;
            }
        }

        public Department SelectedDepartment
        {
            get { return _applicationState.CurrentDepartment != null ? _applicationState.CurrentDepartment.Model : null; }
        }

        public bool IsItemsSelected { get { return SelectedOrders.Any(); } }
        public bool IsItemsSelectedAndUnlocked { get { return SelectedOrders.Any() && SelectedOrders.Count(x => x.Locked) == 0; } }
        public bool IsItemsSelectedAndLocked { get { return SelectedOrders.Any() && SelectedOrders.Count(x => !x.Locked) == 0; } }
        public bool IsNothingSelected { get { return !SelectedOrders.Any(); } }
        public bool IsNothingSelectedAndTicketLocked { get { return !SelectedOrders.Any() && SelectedTicket.IsLocked; } }
        public bool IsNothingSelectedAndTicketTagged { get { return !SelectedOrders.Any() && SelectedTicket.IsTagged; } }
        public bool IsTicketSelected { get { return SelectedTicket != Ticket.Empty; } }

        public OrderViewModel LastSelectedOrder { get; set; }
        public bool ClearSelection { get; set; }

        private IEnumerable<CommandContainerButton> _allAutomationCommands;
        private IEnumerable<CommandContainerButton> AllAutomationCommands
        {
            get
            {
                return _allAutomationCommands ??
                 (_allAutomationCommands =
                  _applicationState.GetAutomationCommands().Select(x => new CommandContainerButton(x, SelectedTicket)).ToList());
            }
        }

        public IEnumerable<CommandContainerButton> TicketAutomationCommands
        {
            get { return AllAutomationCommands.Where(x => x.CommandContainer.DisplayOnTicket && x.CommandContainer.CanDisplay(SelectedTicket)); }
        }

        public IEnumerable<CommandContainerButton> OrderAutomationCommands
        {
            get { return AllAutomationCommands.Where(x => x.CommandContainer.DisplayOnOrders && x.CommandContainer.CanDisplay(SelectedTicket)); }
        }

        public IEnumerable<TicketTagButton> TicketTagButtons
        {
            get
            {
                return _applicationState.CurrentDepartment != null
                    ? _applicationState.GetTicketTagGroups()
                    .OrderBy(x => x.Order)
                    .Select(x => new TicketTagButton(x, SelectedTicket))
                    : null;
            }
        }

        [ImportingConstructor]
        public TicketViewModel(IApplicationState applicationState, IExpressionService expressionService,
            ITicketService ticketService, IAccountService accountService, IResourceService locationService, IUserService userService,
            ICacheService cacheService, IAutomationService automationService, TicketOrdersViewModel ticketOrdersViewModel,
            TicketTotalsViewModel totals, TicketInfoViewModel ticketInfoViewModel, PaymentButtonViewModel paymentButtonViewModel)
        {
            _ticketService = ticketService;
            _userService = userService;
            _cacheService = cacheService;
            _applicationState = applicationState;
            _expressionService = expressionService;
            _automationService = automationService;
            _ticketOrdersViewModel = ticketOrdersViewModel;
            _totals = totals;
            _ticketInfo = ticketInfoViewModel;
            _paymentButtonViewModel = paymentButtonViewModel;

            SelectResourceCommand = new DelegateCommand<ResourceType>(OnSelectResource, CanSelectResource);
            ExecuteAutomationCommnand = new DelegateCommand<CommandContainerButton>(OnExecuteAutomationCommand, CanExecuteAutomationCommand);

            IncQuantityCommand = new CaptionCommand<string>("+", OnIncQuantityCommand, CanIncQuantity);
            DecQuantityCommand = new CaptionCommand<string>("-", OnDecQuantityCommand, CanDecQuantity);
            IncSelectionQuantityCommand = new CaptionCommand<string>("(+)", OnIncSelectionQuantityCommand, CanIncSelectionQuantity);
            DecSelectionQuantityCommand = new CaptionCommand<string>("(-)", OnDecSelectionQuantityCommand, CanDecSelectionQuantity);
            ShowTicketTagsCommand = new CaptionCommand<TicketTagGroup>(Resources.Tag, OnShowTicketsTagExecute, CanExecuteShowTicketTags);
            CancelItemCommand = new CaptionCommand<string>(Resources.Cancel, OnCancelItemCommand);
            MoveOrdersCommand = new CaptionCommand<string>(Resources.MoveTicketLine, OnMoveOrders, CanMoveOrders);
            EditTicketNoteCommand = new CaptionCommand<string>(Resources.TicketNote.Replace(" ", Environment.NewLine), OnEditTicketNote, CanEditTicketNote);
            RemoveTicketLockCommand = new CaptionCommand<string>(Resources.ReleaseLock, OnRemoveTicketLock, CanRemoveTicketLock);
            ChangePriceCommand = new CaptionCommand<string>(Resources.ChangePrice, OnChangePrice, CanChangePrice);

            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderViewModel>>().Subscribe(OnSelectedOrdersChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagData>>().Subscribe(OnTagSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnRefreshTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(OnAccountSelectedFromPopup);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<MenuItemPortion>>().Subscribe(OnPortionSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(OnDepartmentChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<AutomationCommandValueData>>().Subscribe(OnAutomationCommandValueSelected);

            SelectedTicket = Ticket.Empty;
        }

        private bool CanExecuteAutomationCommand(CommandContainerButton arg)
        {
            return arg.IsEnabled && arg.CommandContainer.CanExecute(SelectedTicket) && _expressionService.EvalCommand(FunctionNames.CanExecuteAutomationCommand, arg.CommandContainer.AutomationCommand, new { Ticket = SelectedTicket }, true);
        }

        private void OnExecuteAutomationCommand(CommandContainerButton obj)
        {
            obj.NextValue();
            if (!string.IsNullOrEmpty(obj.CommandContainer.AutomationCommand.Values) && !obj.CommandContainer.AutomationCommand.ToggleValues)
                obj.CommandContainer.AutomationCommand.PublishEvent(EventTopicNames.SelectAutomationCommandValue);
            else
            {
                _automationService.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new { Ticket = SelectedTicket, AutomationCommandName = obj.Name, Value = obj.SelectedValue });
                _ticketOrdersViewModel.SelectedTicket = SelectedTicket;
                ClearSelectedItems();
                ClearSelection = true;
                RefreshVisuals();
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
            }
        }

        private void OnAutomationCommandValueSelected(EventParameters<AutomationCommandValueData> obj)
        {
            _automationService.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new { Ticket = SelectedTicket, AutomationCommandName = obj.Value.AutomationCommand.Name, obj.Value.Value });
            _ticketOrdersViewModel.SelectedTicket = SelectedTicket;
            ClearSelectedItems();
            ClearSelection = true;
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
        }

        private void OnDepartmentChanged(EventParameters<Department> obj)
        {
            _resourceButtons = null;
            RaisePropertyChanged(() => ResourceButtons);
        }

        private void ClearSelectedItems()
        {
            _ticketOrdersViewModel.ClearSelectedOrders();
            RefreshSelectedItems();
        }

        private bool CanSelectResource(ResourceType arg)
        {
            return !SelectedTicket.IsLocked && SelectedTicket.CanSubmit && _applicationState.GetTicketResourceScreens().Any(x => x.ResourceTypeId == arg.Id);
        }

        private void OnSelectResource(ResourceType obj)
        {
            var ticketResource = SelectedTicket.TicketResources.SingleOrDefault(x => x.ResourceTypeId == obj.Id);
            var selectedResource = ticketResource != null ? _cacheService.GetResourceById(ticketResource.ResourceId) : Resource.GetNullResource(obj.Id);
            EntityOperationRequest<Resource>.Publish(selectedResource, EventTopicNames.SelectResource, EventTopicNames.ResourceSelected);
        }

        private void OnPortionSelected(EventParameters<MenuItemPortion> obj)
        {
            if (obj.Topic == EventTopicNames.PortionSelected)
            {
                var taxTemplate = _cacheService.GetMenuItem(x => x.Id == obj.Value.MenuItemId).TaxTemplate;
                SelectedOrder.UpdatePortion(obj.Value, _applicationState.CurrentDepartment.PriceTag, taxTemplate);
            }
        }

        private void OnOrderTagEvent(EventParameters<OrderTagData> obj)
        {
            if (obj.Topic == EventTopicNames.OrderTagSelected)
            {
                _ticketService.TagOrders(SelectedTicket, SelectedTicket.ExtractSelectedOrders(), obj.Value.OrderTagGroup, obj.Value.SelectedOrderTag, "");
                _ticketOrdersViewModel.SelectedTicket = SelectedTicket;
                ClearSelection = true;
                RefreshVisuals();
            }

            if (obj.Topic == EventTopicNames.OrderTagRemoved)
            {
                _ticketService.UntagOrders(SelectedTicket, SelectedTicket.ExtractSelectedOrders(), obj.Value.OrderTagGroup,
                                           obj.Value.SelectedOrderTag);
                _ticketOrdersViewModel.RefreshSelectedOrders();
                RefreshVisuals();
            }
        }

        private void OnRefreshTicket(EventParameters<EventAggregator> obj)
        {
            if (obj.Topic == EventTopicNames.UnlockTicketRequested)
            {
                OnRemoveTicketLock("");
            }

            if (obj.Topic == EventTopicNames.RefreshSelectedTicket)
            {
                RefreshVisuals();
            }
        }

        private void OnAccountSelectedFromPopup(EventParameters<PopupData> obj)
        {
            if (obj.Value.EventMessage == EventTopicNames.SelectResource)
            {
                //todo fix (caller id popupuna týklandýðýnda adisyon açan metod)

                //var dep = AppServices.MainDataContext.Departments.FirstOrDefault(y => y.IsTakeAway);
                //if (dep != null)
                //{
                //    UpdateSelectedDepartment(dep.Id);
                //    SelectedTicketView = OpenTicketListView;
                //}
                //if (SelectedDepartment == null)
                //    SelectedDepartment = AppServices.MainDataContext.Departments.FirstOrDefault();
                RefreshVisuals();
            }
        }

        private void OnTagSelected(EventParameters<TicketTagData> obj)
        {
            if (obj.Topic == EventTopicNames.TicketTagSelected)
            {
                //if (obj.Value.TicketTagGroup != null && obj.Value.TicketTagGroup.Action == 1 && CanCloseTicket(""))
                //    CloseTicketCommand.Execute("");
                //if (obj.Value.TicketTagGroup != null && obj.Value.TicketTagGroup.Action == 2 && CanMakePayment(""))
                //    MakePaymentCommand.Execute("");
                //else
                //{
                RefreshVisuals();
            }
        }

        private void OnSelectedOrdersChanged(EventParameters<OrderViewModel> obj)
        {
            if (obj.Topic == EventTopicNames.SelectedOrdersChanged)
            {
                if (!obj.Value.Selected && !_ticketService.CanDeselectOrder(obj.Value.Model))
                {
                    obj.Value.ToggleSelection();
                    return;
                }

                if (ClearSelection)
                {
                    ClearSelection = false;
                    if (obj.Value != LastSelectedOrder)
                    {
                        ClearSelectedItems();
                        obj.Value.ToggleSelection();
                        return;
                    }
                }

                LastSelectedOrder = obj.Value.Selected ? obj.Value : null;
                if (!SelectedOrders.Any()) LastSelectedOrder = null;
                _ticketOrdersViewModel.UpdateLastSelectedOrder(LastSelectedOrder);

                RefreshSelectedItems();

                var so = new SelectedOrdersData { SelectedOrders = SelectedOrders, Ticket = SelectedTicket };
                so.PublishEvent(EventTopicNames.SelectedOrdersChanged);
            }
        }

        private bool CanExecuteShowTicketTags(TicketTagGroup arg)
        {
            return SelectedTicket.CanSubmit;
        }

        private void OnShowTicketsTagExecute(TicketTagGroup tagGroup)
        {
            if (SelectedTicket == Ticket.Empty)
            {
                tagGroup.PublishEvent(EventTopicNames.ActivateTicketList);
                return;
            }
            var ticketTagData = new TicketTagData
                                    {
                                        TicketTagGroup = tagGroup,
                                        Ticket = SelectedTicket
                                    };
            ticketTagData.PublishEvent(EventTopicNames.SelectTicketTag);
        }

        private bool CanChangePrice(string arg)
        {
            return !SelectedTicket.IsLocked
                && SelectedTicket.CanSubmit
                && SelectedOrder != null
                && (SelectedOrder.Price == 0 || _userService.IsUserPermittedFor(PermissionNames.ChangeItemPrice));
        }

        private void OnChangePrice(string obj)
        {
            decimal price;
            decimal.TryParse(_applicationState.NumberPadValue, out price);
            if (price <= 0)
            {
                InteractionService.UserIntraction.GiveFeedback(Resources.ForChangingPriceTypeAPrice);
            }
            else
            {
                SelectedOrder.UpdatePrice(price, SelectedDepartment.PriceTag);
            }
            ClearSelectedItems();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetNumerator);
        }

        private bool CanRemoveTicketLock(string arg)
        {
            return SelectedTicket.IsLocked &&
                   _userService.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets);
        }

        private void OnRemoveTicketLock(string obj)
        {
            SelectedTicket.UnLock();
            _ticketOrdersViewModel.Refresh();
            _allAutomationCommands = null;
            RefreshVisuals();
        }

        private void OnMoveOrders(string obj)
        {
            SelectedTicket.PublishEvent(EventTopicNames.MoveSelectedOrders);
        }

        private bool CanMoveOrders(string arg)
        {
            if (SelectedTicket.IsLocked) return false;
            if (!SelectedTicket.CanRemoveSelectedOrders(SelectedOrders)) return false;
            if (SelectedOrders.Any(x => x.Id == 0)) return false;
            if (SelectedOrders.Any(x => !x.Locked) && _userService.IsUserPermittedFor(PermissionNames.MoveUnlockedOrders)) return true;
            return _userService.IsUserPermittedFor(PermissionNames.MoveOrders);
        }

        private bool CanEditTicketNote(string arg)
        {
            return !SelectedTicket.IsClosed;
        }

        private void OnEditTicketNote(string obj)
        {
            SelectedTicket.PublishEvent(EventTopicNames.EditTicketNote);
        }

        private void OnDecQuantityCommand(string obj)
        {
            LastSelectedOrder.Quantity--;
        }

        private void OnIncQuantityCommand(string obj)
        {
            LastSelectedOrder.Quantity++;
        }

        private bool CanDecQuantity(string arg)
        {
            return LastSelectedOrder != null &&
                   LastSelectedOrder.Quantity > 1 &&
                   !LastSelectedOrder.IsLocked;
        }

        private bool CanIncQuantity(string arg)
        {
            return LastSelectedOrder != null &&
                   !LastSelectedOrder.IsLocked;
        }

        private bool CanDecSelectionQuantity(string arg)
        {
            return LastSelectedOrder != null &&
                   LastSelectedOrder.Quantity > 1 &&
                   LastSelectedOrder.IsLocked;
        }

        private void OnDecSelectionQuantityCommand(string obj)
        {
            LastSelectedOrder.DecSelectedQuantity();
        }

        private bool CanIncSelectionQuantity(string arg)
        {
            return LastSelectedOrder != null &&
                   LastSelectedOrder.Quantity > 1 &&
                   LastSelectedOrder.IsLocked;
        }

        private void OnIncSelectionQuantityCommand(string obj)
        {
            LastSelectedOrder.IncSelectedQuantity();
        }

        private void OnCancelItemCommand(string obj)
        {
            if (!_ticketOrdersViewModel.CanCancelSelectedOrders())
            {
                ClearSelectedItems();
                return;
            }
            _ticketOrdersViewModel.CancelSelectedOrders();
            _ticketService.RecalculateTicket(SelectedTicket);
            RefreshSelectedItems();
            RefreshSelectedTicket();
        }

        private string _selectedTicketTitle;
        public string SelectedTicketTitle
        {
            get { return _selectedTicketTitle; }
            set { _selectedTicketTitle = value; RaisePropertyChanged(() => SelectedTicketTitle); }
        }

        public void UpdateSelectedTicketTitle()
        {
            _totals.Model = SelectedTicket;
            SelectedTicketTitle = _totals.Title.Trim() == "#" ? Resources.NewTicket : _totals.Title;
        }

        public bool IsTaggedWith(string tagGroup)
        {
            return !string.IsNullOrEmpty(SelectedTicket.GetTagValue(tagGroup));
        }

        public void ResetTicket()
        {
            RefreshVisuals();
            _ticketInfo.Refresh();
            ClearSelectedItems();
        }

        public void RefreshSelectedTicket()
        {
            _totals.Refresh();
            RaisePropertyChanged(() => IsTicketSelected);
            ExecuteAutomationCommnand.RaiseCanExecuteChanged();
        }

        public void RefreshVisuals()
        {
            UpdateSelectedTicketTitle();
            RefreshSelectedTicket();
            RaisePropertyChanged(() => IsNothingSelectedAndTicketLocked);
            RaisePropertyChanged(() => IsNothingSelectedAndTicketTagged);
            RaisePropertyChanged(() => TicketTagButtons);
            RaisePropertyChanged(() => TicketAutomationCommands);
        }

        public void RefreshSelectedItems()
        {
            RaisePropertyChanged(() => IsItemsSelected);
            RaisePropertyChanged(() => IsNothingSelected);
            RaisePropertyChanged(() => IsNothingSelectedAndTicketLocked);
            RaisePropertyChanged(() => IsItemsSelectedAndUnlocked);
            RaisePropertyChanged(() => IsItemsSelectedAndLocked);
            RaisePropertyChanged(() => IsTicketSelected);
            RaisePropertyChanged(() => OrderAutomationCommands);
        }
    }
}
