using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Services;
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
        public ICaptionCommand AddOrderCommand { get; set; }
        public ICaptionCommand ModifyOrderCommand { get; set; }

        public DelegateCommand<EntityType> SelectEntityCommand { get; set; }
        public DelegateCommand<CommandContainerButton> ExecuteAutomationCommnand { get; set; }

        private ObservableCollection<EntityButton> _entityButtons;
        public ObservableCollection<EntityButton> EntityButtons
        {
            get
            {
                if (_entityButtons == null && SelectedDepartment != null && SelectedTicket != null && SelectedTicket.TicketTypeId > 0)
                {
                    _entityButtons = new ObservableCollection<EntityButton>(
                        _cacheService.GetEntityTypesByTicketType(SelectedTicket.TicketTypeId)
                        .Select(x => new EntityButton(x, SelectedTicket)));
                }
                else if (_entityButtons == null && _applicationState.CurrentTicketType != null && _applicationState.CurrentTicketType.Id > 0)
                {
                    _entityButtons = new ObservableCollection<EntityButton>(
                       _cacheService.GetEntityTypesByTicketType(_applicationState.CurrentTicketType.Id)
                       .Select(x => new EntityButton(x, SelectedTicket)));
                }
                return _entityButtons;
            }
        }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _entityButtons = null;
                _allAutomationCommands = null;
                _selectedTicket = value ?? Ticket.Empty;
                _totals.Model = _selectedTicket;
                _ticketOrdersViewModel.SelectedTicket = _selectedTicket;
                _ticketInfo.SelectedTicket = _selectedTicket;
                RaisePropertyChanged(() => EntityButtons);
                RaisePropertyChanged(() => TicketAutomationCommands);
            }
        }

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

        public bool IsPortrait { get { return !_applicationState.IsLandscape; } }
        public bool IsLandscape { get { return _applicationState.IsLandscape; } }
        public bool IsAddOrderButtonVisible { get { return IsPortrait && IsNothingSelected; } }
        public bool IsModifyOrderButtonVisible { get { return IsPortrait && IsItemsSelected; } }
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

        public IEnumerable<CommandContainerButton> UnderTicketAutomationCommands
        {
            get { return AllAutomationCommands.Where(x => x.CommandContainer.DisplayUnderTicket && x.CommandContainer.CanDisplay(SelectedTicket)); }
        }

        public IEnumerable<CommandContainerButton> UnderTicketRow2AutomationCommands
        {
            get { return AllAutomationCommands.Where(x => x.CommandContainer.DisplayUnderTicket2 && x.CommandContainer.CanDisplay(SelectedTicket)); }
        }

        public IEnumerable<TicketTagButton> TicketTagButtons
        {
            get
            {
                return _applicationState.CurrentDepartment != null
                    ? _applicationState.GetTicketTagGroups()
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new TicketTagButton(x, SelectedTicket))
                    : null;
            }
        }

        [ImportingConstructor]
        public TicketViewModel(IApplicationState applicationState, IExpressionService expressionService,
            ITicketService ticketService, IAccountService accountService, IEntityServiceClient locationService, IUserService userService,
            ICacheService cacheService, TicketOrdersViewModel ticketOrdersViewModel,
            TicketTotalsViewModel totals, TicketInfoViewModel ticketInfoViewModel)
        {
            _ticketService = ticketService;
            _userService = userService;
            _cacheService = cacheService;
            _applicationState = applicationState;
            _expressionService = expressionService;
            _ticketOrdersViewModel = ticketOrdersViewModel;
            _totals = totals;
            _ticketInfo = ticketInfoViewModel;

            SelectEntityCommand = new DelegateCommand<EntityType>(OnSelectEntity, CanSelectEntity);
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
            AddOrderCommand = new CaptionCommand<string>(Resources.AddOrder.Replace(" ", Environment.NewLine), OnAddOrder, CanAddOrder);
            ModifyOrderCommand = new CaptionCommand<string>(Resources.ModifyOrder.Replace(" ", Environment.NewLine), OnModifyOrder, CanModifyOrder);

            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderViewModel>>().Subscribe(OnSelectedOrdersChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnRefreshTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<MenuItemPortion>>().Subscribe(OnPortionSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(OnDepartmentChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<AutomationCommandValueData>>().Subscribe(OnAutomationCommandValueSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<AutomationCommandData>>().Subscribe(OnAutomationCommandSelected);

            SelectedTicket = Ticket.Empty;
        }

        private bool CanModifyOrder(string arg)
        {
            return SelectedOrders.Any();
        }

        private void OnModifyOrder(string obj)
        {
            var so = new SelectedOrdersData { SelectedOrders = SelectedOrders, Ticket = SelectedTicket };
            OperationRequest<SelectedOrdersData>.Publish(so, EventTopicNames.DisplayTicketOrderDetails, EventTopicNames.ActivatePosView, "");
        }

        private bool CanAddOrder(string arg)
        {
            return !SelectedTicket.IsClosed && !SelectedTicket.IsLocked;
        }

        private void OnAddOrder(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateMenuView);
        }

        private bool CanExecuteAutomationCommand(CommandContainerButton arg)
        {
            return arg.IsEnabled && arg.CommandContainer.CanExecute(SelectedTicket) && _expressionService.EvalCommand(FunctionNames.CanExecuteAutomationCommand, arg.CommandContainer.AutomationCommand, new { Ticket = SelectedTicket }, true);
        }

        private void OnExecuteAutomationCommand(CommandContainerButton obj)
        {
            ExecuteAutomationCommand(obj.CommandContainer.AutomationCommand, obj.SelectedValue, obj.GetNextValue());
            obj.NextValue();
        }

        private void ExecuteAutomationCommand(AutomationCommand automationCommand, string selectedValue, string nextValue)
        {
            if (!string.IsNullOrEmpty(automationCommand.Values) && !automationCommand.ToggleValues)
                automationCommand.PublishEvent(EventTopicNames.SelectAutomationCommandValue);
            else
            {
                ExecuteAutomationCommand(automationCommand.Name, selectedValue, nextValue);
                RefreshVisuals();
            }
        }

        private void OnAutomationCommandSelected(EventParameters<AutomationCommandData> obj)
        {
            if (obj.Topic == EventTopicNames.HandlerRequested)
            {
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                ExecuteAutomationCommand(obj.Value.AutomationCommand, "", "");
            }
        }

        private void OnAutomationCommandValueSelected(EventParameters<AutomationCommandValueData> obj)
        {
            ExecuteAutomationCommand(obj.Value.AutomationCommand.Name, obj.Value.Value, "");
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
        }

        private void ExecuteAutomationCommand(string automationCommandName, string automationCommandValue, string nextCommandValue)
        {
            if (SelectedOrders.Any())
            {
                foreach (var selectedOrder in SelectedOrders.ToList())
                {
                    _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted,
                                                  new
                                                      {
                                                          Ticket = SelectedTicket,
                                                          Order = selectedOrder,
                                                          AutomationCommandName = automationCommandName,
                                                          CommandValue = automationCommandValue,
                                                          NextCommandValue = nextCommandValue
                                                      });
                }
            }
            else
            {
                _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted,
                                              new
                                                  {
                                                      Ticket = SelectedTicket,
                                                      AutomationCommandName = automationCommandName,
                                                      CommandValue = automationCommandValue,
                                                      NextCommandValue = nextCommandValue
                                                  });
            }
            _ticketOrdersViewModel.SelectedTicket = SelectedTicket;
            ClearSelectedItems();
            ClearSelection = true;
        }

        private void OnDepartmentChanged(EventParameters<Department> obj)
        {
            _entityButtons = null;
            RaisePropertyChanged(() => EntityButtons);
        }

        private void ClearSelectedItems()
        {
            _ticketOrdersViewModel.ClearSelectedOrders();
            RefreshSelectedItems();
        }

        private bool CanSelectEntity(EntityType arg)
        {
            Debug.Assert(SelectedTicket != null);
            return arg != null && !SelectedTicket.IsLocked && SelectedTicket.CanSubmit && _applicationState.GetTicketEntityScreens().Any(x => x.EntityTypeId == arg.Id);
        }

        private void OnSelectEntity(EntityType obj)
        {
            var ticketEntity = SelectedTicket.TicketEntities.SingleOrDefault(x => x.EntityTypeId == obj.Id);
            var selectedEntity = ticketEntity != null ? _cacheService.GetEntityById(ticketEntity.EntityId) : Entity.GetNullEntity(obj.Id);
            OperationRequest<Entity>.Publish(selectedEntity, EventTopicNames.SelectEntity, EventTopicNames.EntitySelected, "");
        }

        private void OnPortionSelected(EventParameters<MenuItemPortion> obj)
        {
            if (obj.Topic == EventTopicNames.PortionSelected)
            {
                var taxTemplate = _applicationState.GetTaxTemplates(obj.Value.MenuItemId);
                SelectedOrder.UpdatePortion(obj.Value, _applicationState.CurrentDepartment.PriceTag, taxTemplate);
                _ticketOrdersViewModel.SelectedTicket = SelectedTicket;
                RefreshVisuals();
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

                if (_applicationState.IsLandscape)
                {
                    var so = new SelectedOrdersData { SelectedOrders = SelectedOrders, Ticket = SelectedTicket };
                    OperationRequest<SelectedOrdersData>.Publish(so, EventTopicNames.DisplayTicketOrderDetails, EventTopicNames.ActivatePosView, "");
                }
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
            _entityButtons = null;
            RaisePropertyChanged(() => EntityButtons);
            RefreshVisuals();
        }

        private void OnMoveOrders(string obj)
        {
            SelectedTicket.PublishEvent(EventTopicNames.MoveSelectedOrders);
        }

        private bool CanMoveOrders(string arg)
        {
            if (SelectedTicket.IsLocked || SelectedTicket.IsClosed) return false;
            if (!SelectedTicket.CanRemoveSelectedOrders(SelectedOrders)) return false;
            if (SelectedOrders.Any(x => x.Id == 0)) return false;
            if (SelectedOrders.Any(x => !x.Locked) && _userService.IsUserPermittedFor(PermissionNames.MoveUnlockedOrders)) return true;
            return _userService.IsUserPermittedFor(PermissionNames.MoveOrders);
        }

        private bool CanEditTicketNote(string arg)
        {
            return SelectedTicket != Ticket.Empty && !SelectedTicket.IsClosed;
        }

        private void OnEditTicketNote(string obj)
        {
            SelectedTicket.PublishEvent(EventTopicNames.EditTicketNote);
        }

        private void OnDecQuantityCommand(string obj)
        {
            LastSelectedOrder.Quantity--;
            RefreshSelectedOrders();
        }

        private void OnIncQuantityCommand(string obj)
        {
            LastSelectedOrder.Quantity++;
            RefreshSelectedOrders();
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
            RefreshSelectedOrders();
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
            RefreshSelectedOrders();
        }

        private void OnCancelItemCommand(string obj)
        {
            if (!_ticketOrdersViewModel.CanCancelSelectedOrders())
            {
                ClearSelectedItems();
                return;
            }
            _ticketService.CancelSelectedOrders(SelectedTicket);
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
            var result = _totals.TitleWithAccountBalancesAndState;
            SelectedTicketTitle = result.Trim() == "#" ? Resources.NewTicket : result;
        }

        public bool IsTaggedWith(string tagGroup)
        {
            return !string.IsNullOrEmpty(SelectedTicket.GetTagValue(tagGroup));
        }

        public void ResetTicket()
        {
            RefreshVisuals();
            _totals.ResetCache();
            RefreshSelectedTicketTitle();
            ClearSelectedItems();
        }

        public void RefreshSelectedTicket()
        {
            _totals.Refresh();
            RaisePropertyChanged(() => IsTicketSelected);
            ExecuteAutomationCommnand.RaiseCanExecuteChanged();
        }

        public void RefreshSelectedTicketTitle()
        {
            _ticketInfo.Refresh();
            UpdateSelectedTicketTitle();
            RefreshVisuals();
        }

        public void RefreshVisuals()
        {
            RefreshSelectedTicket();
            RaisePropertyChanged(() => IsNothingSelectedAndTicketLocked);
            RaisePropertyChanged(() => IsNothingSelectedAndTicketTagged);
            RaisePropertyChanged(() => TicketTagButtons);
            RaisePropertyChanged(() => TicketAutomationCommands);
            RaisePropertyChanged(() => UnderTicketAutomationCommands);
            RaisePropertyChanged(() => UnderTicketRow2AutomationCommands);
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
            RaisePropertyChanged(() => IsAddOrderButtonVisible);
            RaisePropertyChanged(() => IsModifyOrderButtonVisible);
        }

        public void RefreshLayout()
        {
            RaisePropertyChanged(() => IsPortrait);
            RaisePropertyChanged(() => IsLandscape);
            RaisePropertyChanged(() => IsAddOrderButtonVisible);
            RaisePropertyChanged(() => IsModifyOrderButtonVisible);
        }

        private void RefreshSelectedOrders()
        {
            _ticketOrdersViewModel.RefreshSelectedOrders();
            RefreshVisuals();
        }
    }
}
