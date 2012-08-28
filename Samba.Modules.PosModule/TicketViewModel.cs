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
using Samba.Presentation.Common.Interaction;
using Samba.Presentation.Common.Services;
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
        private readonly IApplicationState _applicationState;
        private readonly IAutomationService _automationService;
        private readonly ICacheService _cacheService;
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
        public ICaptionCommand ShowOrderTagsCommand { get; set; }
        public ICaptionCommand CancelItemCommand { get; set; }
        public ICaptionCommand EditTicketNoteCommand { get; set; }
        public ICaptionCommand RemoveTicketLockCommand { get; set; }
        public ICaptionCommand RemoveTicketTagCommand { get; set; }
        public ICaptionCommand ChangePriceCommand { get; set; }

        public DelegateCommand<ResourceTemplate> SelectResourceCommand { get; set; }
        public DelegateCommand<CommandContainerButton> ExecuteAutomationCommnand { get; set; }

        private ObservableCollection<ResourceButton> _resourceButtons;
        public ObservableCollection<ResourceButton> ResourceButtons
        {
            get
            {
                if (_resourceButtons == null && SelectedDepartment != null)
                {
                    _resourceButtons = new ObservableCollection<ResourceButton>(
                        SelectedDepartment.ResourceScreens
                        .OrderBy(x => x.Order)
                        .Select(x => _cacheService.GetResourceTemplateById(x.ResourceTemplateId))
                        .Distinct()
                        .Select(x => new ResourceButton(x, SelectedTicket)));
                }
                return _resourceButtons;
            }
        }

        public ObservableCollection<ICaptionCommand> CustomOrderCommands { get { return PresentationServices.OrderCommands; } }
        public ObservableCollection<ICaptionCommand> CustomTicketCommands { get { return PresentationServices.TicketCommands; } }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _resourceButtons = null;
                _automationCommands = null;
                _selectedTicket = value ?? Ticket.Empty;
                _totals.Model = _selectedTicket;
                _ticketOrdersViewModel.SelectedTicket = _selectedTicket;
                _ticketInfo.SelectedTicket = _selectedTicket;
                _paymentButtonViewModel.SelectedTicket = _selectedTicket;
                RaisePropertyChanged(() => ResourceButtons);
                RaisePropertyChanged(() => AutomationCommands);
            }
        }

        private readonly TicketInfoViewModel _ticketInfo;
        private readonly PaymentButtonViewModel _paymentButtonViewModel;
        public TicketInfoViewModel TicketInfo { get { return _ticketInfo; } }

        public IList<Order> SelectedOrders { get { return _ticketOrdersViewModel.SelectedOrderModels; } }

        public Order SelectedOrder
        {
            get
            {
                return _ticketOrdersViewModel != null && SelectedOrders.Count == 1 ? SelectedOrders[0] : null;
            }
        }

        public Department SelectedDepartment
        {
            get { return _applicationState.CurrentDepartment != null ? _applicationState.CurrentDepartment.Model : null; }
        }

        public bool IsItemsSelected { get { return SelectedOrders.Count() > 0; } }
        public bool IsItemsSelectedAndUnlocked { get { return SelectedOrders.Count() > 0 && SelectedOrders.Count(x => x.Locked) == 0; } }
        public bool IsItemsSelectedAndLocked { get { return SelectedOrders.Count() > 0 && SelectedOrders.Count(x => !x.Locked) == 0; } }
        public bool IsNothingSelected { get { return SelectedOrders.Count() == 0; } }
        public bool IsNothingSelectedAndTicketLocked { get { return SelectedOrders.Count() == 0 && SelectedTicket.Locked; } }
        public bool IsNothingSelectedAndTicketTagged { get { return SelectedOrders.Count() == 0 && SelectedTicket.IsTagged; } }
        public bool IsTicketSelected { get { return SelectedTicket != Ticket.Empty; } }

        public OrderViewModel LastSelectedOrder { get; set; }
        public bool ClearSelection { get; set; }

        private IEnumerable<CommandContainerButton> _automationCommands;
        public IEnumerable<CommandContainerButton> AutomationCommands
        {
            get
            {
                return _automationCommands ?? (_automationCommands = _cacheService.GetAutomationCommands().Select(x => new CommandContainerButton(x, SelectedTicket)));
            }
        }

        public IEnumerable<TicketTagButton> TicketTagButtons
        {
            get
            {
                return _applicationState.CurrentDepartment != null
                    ? _cacheService.GetTicketTagGroups()
                    .OrderBy(x => x.Order)
                    .Select(x => new TicketTagButton(x, SelectedTicket))
                    : null;
            }
        }

        public IEnumerable<OrderStateButton> OrderStateButtons
        {
            get
            {
                if (SelectedOrders.Count() > 0)
                {
                    return _cacheService.GetOrderStateGroups()
                        .Where(x => !string.IsNullOrEmpty(x.ButtonHeader))
                        .Select(x => new OrderStateButton(x));
                }
                return null;
            }
        }

        public IEnumerable<OrderTagButton> OrderTagButtons
        {
            get
            {
                if (SelectedOrders.Count() > 0)
                {
                    return _cacheService.GetOrderTagGroupsForItems(SelectedOrders.Select(x => x.MenuItemId))
                        .Where(x => !string.IsNullOrEmpty(x.ButtonHeader))
                        .Select(x => new OrderTagButton(x));
                }
                return null;
            }
        }

        [ImportingConstructor]
        public TicketViewModel(IApplicationState applicationState,
            ITicketService ticketService, IAccountService accountService, IResourceService locationService, IUserService userService,
            IAutomationService automationService, ICacheService cacheService, TicketOrdersViewModel ticketOrdersViewModel,
            TicketTotalsViewModel totals, TicketInfoViewModel ticketInfoViewModel, PaymentButtonViewModel paymentButtonViewModel)
        {
            _ticketService = ticketService;
            _userService = userService;
            _applicationState = applicationState;
            _automationService = automationService;
            _cacheService = cacheService;
            _ticketOrdersViewModel = ticketOrdersViewModel;
            _totals = totals;
            _ticketInfo = ticketInfoViewModel;
            _paymentButtonViewModel = paymentButtonViewModel;

            SelectResourceCommand = new DelegateCommand<ResourceTemplate>(OnSelectResource, CanSelectResource);
            ExecuteAutomationCommnand = new DelegateCommand<CommandContainerButton>(OnExecuteAutomationCommand, CanExecuteAutomationCommand);

            IncQuantityCommand = new CaptionCommand<string>("+", OnIncQuantityCommand, CanIncQuantity);
            DecQuantityCommand = new CaptionCommand<string>("-", OnDecQuantityCommand, CanDecQuantity);
            IncSelectionQuantityCommand = new CaptionCommand<string>("(+)", OnIncSelectionQuantityCommand, CanIncSelectionQuantity);
            DecSelectionQuantityCommand = new CaptionCommand<string>("(-)", OnDecSelectionQuantityCommand, CanDecSelectionQuantity);
            ShowTicketTagsCommand = new CaptionCommand<TicketTagGroup>(Resources.Tag, OnShowTicketsTagExecute, CanExecuteShowTicketTags);
            ShowOrderStatesCommand = new CaptionCommand<OrderStateGroup>(Resources.Tag, OnShowOrderStatesExecute, CanShowOrderStatesExecute);
            ShowOrderTagsCommand = new CaptionCommand<OrderTagGroup>(Resources.Tag, OnShowOrderTagsExecute, CanShowOrderTagsExecute);
            CancelItemCommand = new CaptionCommand<string>(Resources.Cancel, OnCancelItemCommand, CanCancelSelectedItems);
            MoveOrdersCommand = new CaptionCommand<string>(Resources.MoveTicketLine, OnMoveOrders, CanMoveOrders);
            EditTicketNoteCommand = new CaptionCommand<string>(Resources.TicketNote, OnEditTicketNote, CanEditTicketNote);
            RemoveTicketLockCommand = new CaptionCommand<string>(Resources.ReleaseLock, OnRemoveTicketLock, CanRemoveTicketLock);
            ChangePriceCommand = new CaptionCommand<string>(Resources.ChangePrice, OnChangePrice, CanChangePrice);

            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderViewModel>>().Subscribe(OnSelectedOrdersChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagData>>().Subscribe(OnTagSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnRefreshTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(OnAccountSelectedFromPopup);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderStateData>>().Subscribe(OnOrderStateEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<MenuItemPortion>>().Subscribe(OnPortionSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(OnDepartmentChanged);

            SelectedTicket = Ticket.Empty;
        }

        private bool CanExecuteAutomationCommand(CommandContainerButton arg)
        {
            return arg.IsEnabled;
        }

        private void OnExecuteAutomationCommand(CommandContainerButton obj)
        {
            obj.NextValue();
            _automationService.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new { Ticket = SelectedTicket, AutomationCommandName = obj.Name, Value = obj.SelectedValue });
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

        private bool CanSelectResource(ResourceTemplate arg)
        {
            return !SelectedTicket.Locked && SelectedTicket.CanSubmit && _applicationState.CurrentDepartment.ResourceScreens.Any(x => x.ResourceTemplateId == arg.Id);
        }

        private void OnSelectResource(ResourceTemplate obj)
        {
            var ticketResource = SelectedTicket.TicketResources.SingleOrDefault(x => x.ResourceTemplateId == obj.Id);
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

        private void OnOrderStateEvent(EventParameters<OrderStateData> obj)
        {
            if (obj.Topic == EventTopicNames.OrderStateSelected)
            {
                _ticketOrdersViewModel.FixSelectedItems();
                _ticketOrdersViewModel.SelectedOrders.ToList().ForEach(x => x.UpdateOrderState(obj.Value.OrderStateGroup, obj.Value.SelectedOrderState, _applicationState.CurrentLoggedInUser.Id));
                ClearSelectedItems();
                RefreshVisuals();
            }
        }

        private void OnOrderTagEvent(EventParameters<OrderTagData> obj)
        {
            if (obj.Topic == EventTopicNames.OrderTagSelected)
            {
                _ticketOrdersViewModel.FixSelectedItems();
                _ticketService.TagOrders(_ticketOrdersViewModel.SelectedOrderModels, obj.Value.OrderTagGroup, obj.Value.SelectedOrderTag);
                if (obj.Value.OrderTagGroup.MaxSelectedItems == 1)
                    ClearSelectedItems();
                _ticketOrdersViewModel.RefreshSelectedOrders();
                ClearSelection = true;
                RefreshVisuals();
            }

            if (obj.Topic == EventTopicNames.OrderTagRemoved)
            {
                _ticketService.UntagOrders(_ticketOrdersViewModel.SelectedOrderModels, obj.Value.OrderTagGroup,
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
                if (SelectedOrders.Count() == 0) LastSelectedOrder = null;
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

        private void OnShowOrderStatesExecute(OrderStateGroup orderStateGroup)
        {
            var orderStateData = new OrderStateData
                                   {
                                       SelectedOrders = SelectedOrders,
                                       OrderStateGroup = orderStateGroup,
                                       Ticket = SelectedTicket
                                   };
            orderStateData.PublishEvent(EventTopicNames.SelectOrderState);
        }

        private bool CanShowOrderStatesExecute(OrderStateGroup arg)
        {
            if (SelectedOrders.Count() == 0) return false;
            if (!arg.DecreaseOrderInventory && SelectedOrders.Any(x => !x.Locked && !x.IsStateApplied(arg))) return false;
            if (!arg.CalculateOrderPrice && !SelectedTicket.CanRemoveSelectedOrders(SelectedOrders)) return false;
            if (SelectedOrders.Any(x => !x.DecreaseInventory && !x.IsStateApplied(arg))) return false;
            return !arg.UnlocksOrder || !SelectedOrders.Any(x => x.Locked && x.OrderTagValues.Count(y => y.OrderTagGroupId == arg.Id) > 0);
        }

        private void OnShowOrderTagsExecute(OrderTagGroup orderTagGroup)
        {
            var orderTagData = new OrderTagData
            {
                SelectedOrders = SelectedOrders,
                OrderTagGroup = orderTagGroup,
                Ticket = SelectedTicket
            };
            orderTagData.PublishEvent(EventTopicNames.SelectOrderTag);
        }

        private bool CanShowOrderTagsExecute(OrderTagGroup arg)
        {
            if (SelectedOrders.Count() == 0) return false;
            return true;
        }

        private bool CanChangePrice(string arg)
        {
            return !SelectedTicket.Locked
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
            return SelectedTicket.Locked &&
                   _userService.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets);
        }

        private void OnRemoveTicketLock(string obj)
        {
            SelectedTicket.Locked = false;
            _ticketOrdersViewModel.Refresh();
            _automationCommands = null;
            RefreshVisuals();
        }

        private void OnMoveOrders(string obj)
        {
            SelectedTicket.PublishEvent(EventTopicNames.MoveSelectedOrders);
        }

        private bool CanMoveOrders(string arg)
        {
            if (SelectedTicket.Locked) return false;
            if (!SelectedTicket.CanRemoveSelectedOrders(SelectedOrders)) return false;
            if (SelectedOrders.Any(x => x.Id == 0)) return false;
            if (SelectedOrders.Any(x => !x.Locked) && _userService.IsUserPermittedFor(PermissionNames.MoveUnlockedOrders)) return true;
            return _userService.IsUserPermittedFor(PermissionNames.MoveOrders);
        }

        private bool CanEditTicketNote(string arg)
        {
            return !SelectedTicket.IsPaid;
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

        private bool CanCancelSelectedItems(string arg)
        {
            return true;//_ticketOrdersViewModel.CanCancelSelectedOrders();
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
            RaisePropertyChanged(() => AutomationCommands);
        }

        public void RefreshSelectedItems()
        {
            RaisePropertyChanged(() => IsItemsSelected);
            RaisePropertyChanged(() => IsNothingSelected);
            RaisePropertyChanged(() => IsNothingSelectedAndTicketLocked);
            RaisePropertyChanged(() => IsItemsSelectedAndUnlocked);
            RaisePropertyChanged(() => IsItemsSelectedAndLocked);
            RaisePropertyChanged(() => IsTicketSelected);
            RaisePropertyChanged(() => OrderStateButtons);
            RaisePropertyChanged(() => OrderTagButtons);
        }
    }
}
