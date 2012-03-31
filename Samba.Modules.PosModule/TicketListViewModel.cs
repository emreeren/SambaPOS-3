using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Infrastructure.Settings;
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
    public class TicketListViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IPrinterService _printerService;
        private readonly IAccountService _accountService;
        private readonly ILocationService _locationService;
        private readonly IUserService _userService;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IAutomationService _automationService;
        private readonly ICacheService _cacheService;
        private readonly TicketOrdersViewModel _ticketOrdersViewModel;

        public DelegateCommand<ScreenMenuItemData> AddMenuItemCommand { get; set; }
        public CaptionCommand<string> CloseTicketCommand { get; set; }
        public CaptionCommand<string> MakePaymentCommand { get; set; }

        public CaptionCommand<PaymentTemplate> MakeFastPaymentCommand { get; set; }
        public CaptionCommand<string> SelectAccountCommand { get; set; }
        public CaptionCommand<string> SelectTargetAccountCommand { get; set; }
        public CaptionCommand<string> PrintTicketCommand { get; set; }
        public CaptionCommand<string> PrintInvoiceCommand { get; set; }
        public CaptionCommand<string> ShowAllOpenTickets { get; set; }
        public CaptionCommand<string> MoveOrdersCommand { get; set; }

        public ICaptionCommand IncQuantityCommand { get; set; }
        public ICaptionCommand DecQuantityCommand { get; set; }
        public ICaptionCommand IncSelectionQuantityCommand { get; set; }
        public ICaptionCommand DecSelectionQuantityCommand { get; set; }
        public ICaptionCommand ShowTicketTagsCommand { get; set; }
        public ICaptionCommand ShowOrderTagsCommand { get; set; }
        public ICaptionCommand CancelItemCommand { get; set; }
        public ICaptionCommand EditTicketNoteCommand { get; set; }
        public ICaptionCommand RemoveTicketLockCommand { get; set; }
        public ICaptionCommand RemoveTicketTagCommand { get; set; }
        public ICaptionCommand ChangePriceCommand { get; set; }
        public ICaptionCommand PrintJobCommand { get; set; }

        public ObservableCollection<ICaptionCommand> CustomOrderCommands { get { return PresentationServices.OrderCommands; } }
        public ObservableCollection<ICaptionCommand> CustomTicketCommands { get { return PresentationServices.TicketCommands; } }
        public PaymentButtonGroupViewModel PaymentButtonGroup { get; set; }

        private TicketViewModel _selectedTicket;
        public TicketViewModel SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                _ticketOrdersViewModel.SelectedTicket = value;
                RaisePropertyChanged(() => SelectedTicket);
            }
        }

        private TicketTotalsViewModel _totals;
        public TicketTotalsViewModel Totals
        {
            get { return _totals; }
            set
            {
                _totals = value;
                RaisePropertyChanged(() => Totals);
            }
        }

        private readonly ObservableCollection<Order> _selectedOrders;
        public Order SelectedOrder
        {
            get
            {
                return SelectedTicket != null && SelectedTicket.SelectedOrders.Count == 1 ? SelectedTicket.SelectedOrders[0].Model : null;
            }
        }

        public IEnumerable<PrintJobButton> PrintJobButtons
        {
            get
            {
                return SelectedTicket != null
                    ? SelectedTicket.PrintJobButtons.Where(x => x.Model.UseFromPos)
                    : null;
            }
        }

        public Department SelectedDepartment
        {
            get { return _applicationState.CurrentDepartment; }
            set
            {
                if (value != _applicationState.CurrentDepartment)
                {
                    _applicationStateSetter.SetCurrentDepartment(value != null ? value.Id : 0);
                    RaisePropertyChanged(() => SelectedDepartment);
                    RaisePropertyChanged(() => SelectedTicket);
                    SelectedDepartment.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
                }
            }
        }

        public bool CanDisplayAllTickets { get { return SelectedTicket == null; } }
        public bool IsItemsSelected { get { return _selectedOrders.Count > 0; } }
        public bool IsItemsSelectedAndUnlocked { get { return _selectedOrders.Count > 0 && _selectedOrders.Where(x => x.Locked).Count() == 0; } }
        public bool IsItemsSelectedAndLocked { get { return _selectedOrders.Count > 0 && _selectedOrders.Where(x => !x.Locked).Count() == 0; } }
        public bool IsNothingSelected { get { return _selectedOrders.Count == 0; } }
        public bool IsNothingSelectedAndTicketLocked { get { return _selectedTicket != null && _selectedOrders.Count == 0 && _selectedTicket.IsLocked; } }
        public bool IsNothingSelectedAndTicketTagged { get { return _selectedTicket != null && _selectedOrders.Count == 0 && SelectedTicket.IsTagged; } }
        public bool IsTicketSelected { get { return SelectedTicket != null && _selectedOrders.Count == 0; } }

        public bool IsLocationButtonVisible
        {
            get
            {
                return
                ((_locationService.GetLocationCount() > 0 ||
                    (_applicationState.CurrentDepartment != null
                    && _applicationState.CurrentDepartment.IsAlaCarte))
                    && IsNothingSelected) &&
                    ((_applicationState.CurrentDepartment != null &&
                    _applicationState.CurrentDepartment.LocationScreens.Count > 0));
            }
        }

        public bool IsAccountButtonVisible
        {
            get
            {
                return _accountService.GetAccountCount() > 0
                    || (_applicationState.CurrentDepartment != null && _applicationState.CurrentDepartment.IsTakeAway)
                    && IsNothingSelected;
            }
        }

        public OrderViewModel LastSelectedOrder { get; set; }

        public IEnumerable<TicketTagButton> TicketTagButtons
        {
            get
            {
                return _applicationState.CurrentDepartment != null
                    ? _applicationState.CurrentDepartment.TicketTemplate.TicketTagGroups
                    .Where(x => x.ActiveOnPosClient)
                    .OrderBy(x => x.Order)
                    .Select(x => new TicketTagButton(x, SelectedTicket != null ? SelectedTicket.Model : null))
                    : null;
            }
        }

        public IEnumerable<OrderTagGroupButton> OrderTagButtons
        {
            get
            {
                if (_selectedOrders != null && _selectedOrders.Count > 0)
                {
                    return _cacheService.GetOrderTagGroupsForItems(_selectedOrders.Select(x => x.MenuItemId))
                        .Where(x => !string.IsNullOrEmpty(x.ButtonHeader))
                        .Select(x => new OrderTagGroupButton(x));
                }
                return null;
            }
        }

        [ImportingConstructor]
        public TicketListViewModel(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ITicketService ticketService, IAccountService accountService, IPrinterService printerService,
            ILocationService locationService, IUserService userService, IAutomationService automationService,
            ICacheService cacheService, TicketOrdersViewModel ticketOrdersViewModel, TicketTotalsViewModel totals)
        {
            _printerService = printerService;
            _ticketService = ticketService;
            _accountService = accountService;
            _locationService = locationService;
            _userService = userService;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _automationService = automationService;
            _cacheService = cacheService;
            _ticketOrdersViewModel = ticketOrdersViewModel;
            _totals = totals;

            _selectedOrders = new ObservableCollection<Order>();

            PrintJobCommand = new CaptionCommand<PrintJob>(Resources.Print, OnPrintJobExecute, CanExecutePrintJob);

            AddMenuItemCommand = new DelegateCommand<ScreenMenuItemData>(OnAddMenuItemCommandExecute);
            CloseTicketCommand = new CaptionCommand<string>(Resources.CloseTicket_r, OnCloseTicketExecute, CanCloseTicket);
            MakePaymentCommand = new CaptionCommand<string>(Resources.Settle, OnMakePaymentExecute, CanMakePayment);
            MakeFastPaymentCommand = new CaptionCommand<PaymentTemplate>("[FastPayment]", OnMakeFastPaymentExecute, CanMakeFastPayment);
            SelectAccountCommand = new CaptionCommand<string>(Resources.SelectAccount, OnSelectAccountExecute, CanSelectAccount);
            SelectTargetAccountCommand = new CaptionCommand<string>("Select Target", OnSelectTargetAccount);
            ShowAllOpenTickets = new CaptionCommand<string>(Resources.AllTickets_r, OnShowAllOpenTickets);

            IncQuantityCommand = new CaptionCommand<string>("+", OnIncQuantityCommand, CanIncQuantity);
            DecQuantityCommand = new CaptionCommand<string>("-", OnDecQuantityCommand, CanDecQuantity);
            IncSelectionQuantityCommand = new CaptionCommand<string>("(+)", OnIncSelectionQuantityCommand, CanIncSelectionQuantity);
            DecSelectionQuantityCommand = new CaptionCommand<string>("(-)", OnDecSelectionQuantityCommand, CanDecSelectionQuantity);
            ShowTicketTagsCommand = new CaptionCommand<TicketTagGroup>(Resources.Tag, OnShowTicketsTagExecute, CanExecuteShowTicketTags);
            ShowOrderTagsCommand = new CaptionCommand<OrderTagGroup>(Resources.Tag, OnShowOrderTagsExecute, CanShowOrderTagsExecute);
            CancelItemCommand = new CaptionCommand<string>(Resources.Cancel, OnCancelItemCommand, CanCancelSelectedItems);
            MoveOrdersCommand = new CaptionCommand<string>(Resources.MoveTicketLine, OnMoveOrders, CanMoveOrders);
            EditTicketNoteCommand = new CaptionCommand<string>(Resources.TicketNote, OnEditTicketNote, CanEditTicketNote);
            RemoveTicketLockCommand = new CaptionCommand<string>(Resources.ReleaseLock, OnRemoveTicketLock, CanRemoveTicketLock);
            ChangePriceCommand = new CaptionCommand<string>(Resources.ChangePrice, OnChangePrice, CanChangePrice);

            PaymentButtonGroup = new PaymentButtonGroupViewModel(MakeFastPaymentCommand, MakePaymentCommand, CloseTicketCommand);

            EventServiceFactory.EventService.GetEvent<GenericEvent<ScreenMenuItemData>>().Subscribe(OnMenuItemSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderViewModel>>().Subscribe(OnSelectedOrdersChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagData>>().Subscribe(OnTagSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(OnAccountSelectedForTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<ResourceScreenItem>>>().Subscribe(OnAccountScreenItemSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnRefreshTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(OnMessageReceived);
            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(OnAccountSelectedFromPopup);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<MenuItemPortion>>().Subscribe(OnPortionSelected);
            EventServiceFactory.EventService.GetEvent<GenericIdEvent>().Subscribe(OnTicketIdPublished);
        }

        private void ClearSelectedItems()
        {
            if (SelectedTicket != null) SelectedTicket.ClearSelectedItems();
            _selectedOrders.Clear();
            RefreshSelectedItems();
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
                SelectedTicket.FixSelectedItems();
                SelectedTicket.SelectedOrders.ToList().ForEach(x =>
                    x.ToggleOrderTag(obj.Value.OrderTagGroup, obj.Value.SelectedOrderTag, _applicationState.CurrentLoggedInUser.Id));
                if (!string.IsNullOrEmpty(obj.Value.OrderTagGroup.ButtonHeader) && obj.Value.OrderTagGroup.IsSingleSelection)
                    ClearSelectedItems();
                RefreshVisuals();
            }
        }

        private void OnRefreshTicket(EventParameters<EventAggregator> obj)
        {
            if (obj.Topic == EventTopicNames.PaymentSubmitted)
            {
                CloseTicket();
            }

            if (obj.Topic == EventTopicNames.ActivatePosView)
            {
                RefreshVisuals();
                ClearSelectedItems();
            }

            if (obj.Topic == EventTopicNames.RefreshSelectedTicket)
            {
                RefreshVisuals();
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
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

        private void OnMessageReceived(EventParameters<Message> obj)
        {
            if (_applicationState.ActiveAppScreen == AppScreens.TicketList
                && obj.Topic == EventTopicNames.MessageReceivedEvent
                && obj.Value.Command == Messages.TicketRefreshMessage)
            {
                SelectedDepartment.PublishEvent(EventTopicNames.ActivateOpenTickets);
                RefreshVisuals();
            }
        }

        private void OnTicketIdPublished(EventParameters<int> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayTicket)
            {
                if (_selectedTicket != null)
                    _ticketService.CloseTicket(_selectedTicket.Model);
                OpenTicket(obj.Value);
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
            }
        }

        private void OnAccountSelectedForTicket(EventParameters<EntityOperationRequest<Resource>> obj)
        {
            if (obj.Topic == EventTopicNames.ResourceSelected)
            {
                if (SelectedTicket == null) OpenTicket(0);
                if (SelectedTicket != null)
                {
                    _ticketService.UpdateResource(SelectedTicket.Model, obj.Value.SelectedEntity);
                    RefreshVisuals();
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
                }
            }

            //if (obj.Topic == EventTopicNames.TargetAccountSelected)
            //{
            //    if (SelectedTicket == null) OpenTicket(0);
            //    if (SelectedTicket != null)
            //    {
            //        _ticketService.UpdateTargetAccount(SelectedTicket.Model, obj.Value.SelectedEntity);
            //        RefreshVisuals();
            //        EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
            //    }
            //}

            //if (obj.Topic == EventTopicNames.PaymentRequestedForTicket)
            //{
            //    if (SelectedTicket != null)
            //    {
            //        _ticketService.UpdateAccount(SelectedTicket.Model, obj.Value);
            //        if (!string.IsNullOrEmpty(SelectedTicket.AccountName) && SelectedTicket.Orders.Count > 0)
            //            MakePaymentCommand.Execute("");
            //        else
            //        {
            //            RefreshVisuals();
            //            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
            //        }
            //    }
            //}
        }

        private void OnTagSelected(EventParameters<TicketTagData> obj)
        {
            if (obj.Topic == EventTopicNames.TagSelectedForSelectedTicket)
            {
                if (obj.Value.TicketTagGroup != null && obj.Value.TicketTagGroup.Action == 1 && CanCloseTicket(""))
                    CloseTicketCommand.Execute("");
                if (obj.Value.TicketTagGroup != null && obj.Value.TicketTagGroup.Action == 2 && CanMakePayment(""))
                    MakePaymentCommand.Execute("");
                else
                {
                    RefreshVisuals();
                }
            }
        }

        private void OnSelectedOrdersChanged(EventParameters<OrderViewModel> obj)
        {
            if (SelectedTicket != null && obj.Topic == EventTopicNames.SelectedOrdersChanged)
            {
                LastSelectedOrder = obj.Value.Selected ? obj.Value : null;
                foreach (var item in SelectedTicket.SelectedOrders)
                {
                    item.IsLastSelected = item == LastSelectedOrder;
                }

                _selectedOrders.Clear();
                _selectedOrders.AddRange(SelectedTicket.SelectedOrders.Select(x => x.Model));
                if (_selectedOrders.Count == 0) LastSelectedOrder = null;

                RefreshSelectedItems();

                var so = new SelectedOrdersData { SelectedOrders = _selectedOrders, Ticket = SelectedTicket.Model };
                so.PublishEvent(EventTopicNames.SelectedOrdersChanged);

            }
        }

        private void RefreshSelectedItems()
        {
            RaisePropertyChanged(() => IsItemsSelected);
            RaisePropertyChanged(() => IsNothingSelected);
            RaisePropertyChanged(() => IsNothingSelectedAndTicketLocked);
            RaisePropertyChanged(() => IsLocationButtonVisible);
            RaisePropertyChanged(() => IsAccountButtonVisible);
            RaisePropertyChanged(() => IsItemsSelectedAndUnlocked);
            RaisePropertyChanged(() => IsItemsSelectedAndLocked);
            RaisePropertyChanged(() => IsTicketSelected);
            RaisePropertyChanged(() => OrderTagButtons);
        }

        private void OnMenuItemSelected(EventParameters<ScreenMenuItemData> obj)
        {
            if (obj.Topic == EventTopicNames.ScreenMenuItemDataSelected) AddMenuItemCommand.Execute(obj.Value);
        }

        private void OnAccountScreenItemSelected(EventParameters<EntityOperationRequest<ResourceScreenItem>> obj)
        {
            if (obj.Topic == EventTopicNames.LocationSelectedForTicket)
            {
                if (SelectedTicket != null)
                {
                    _ticketService.UpdateResource(SelectedTicket.Model, obj.Value.SelectedEntity.Resource);
                    CloseTicket();

                    if (!_applicationState.CurrentTerminal.AutoLogout)
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
                }
                else
                {
                    var openTickets = _ticketService.GetOpenTickets(obj.Value.SelectedEntity.Resource.Id);
                    if (openTickets.Count() == 0)
                    {
                        OpenTicket(0);
                        if (SelectedTicket != null)
                        {
                            _ticketService.UpdateResource(SelectedTicket.Model, obj.Value.SelectedEntity.Resource);
                        }
                    }
                    else
                    {
                        OpenTicket(openTickets.ElementAt(0));
                    }
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
                }
            }
        }

        private bool CanExecuteShowTicketTags(TicketTagGroup arg)
        {
            return SelectedTicket == null || (SelectedTicket.Model.CanSubmit);
        }

        private void OnShowTicketsTagExecute(TicketTagGroup tagGroup)
        {
            if (SelectedTicket != null)
            {
                var ticketTagData = new TicketTagData
                                        {
                                            TicketTagGroup = tagGroup,
                                            Ticket = SelectedTicket.Model
                                        };
                ticketTagData.PublishEvent(EventTopicNames.SelectTicketTag);
            }
            else if (ShowAllOpenTickets.CanExecute(""))
            {
                SelectedDepartment.PublishEvent(EventTopicNames.ActivateOpenTickets);
                //todo bi çözüm bulalým
                //if ((OpenTickets != null && OpenTickets.Count() > 0))
                //{
                //    SelectedTicketView = OpenTicketListView;
                //    RaisePropertyChanged(() => OpenTickets);
                //}
                //else InteractionService.UserIntraction.GiveFeedback(string.Format(Resources.NoTicketsFoundForTag, tagGroup.Name));
            }
        }

        private void OnShowOrderTagsExecute(OrderTagGroup orderTagGroup)
        {
            var orderTagData = new OrderTagData
                                   {
                                       SelectedOrders = _selectedOrders,
                                       OrderTagGroup = orderTagGroup,
                                       Ticket = SelectedTicket.Model
                                   };
            orderTagData.PublishEvent(EventTopicNames.SelectOrderTag);
        }

        private bool CanShowOrderTagsExecute(OrderTagGroup arg)
        {
            if (_selectedOrders.Count == 0) return false;
            if (!arg.DecreaseOrderInventory && _selectedOrders.Any(x => !x.Locked && !x.IsTaggedWith(arg))) return false;
            if (_selectedOrders.Any(x => !x.DecreaseInventory && !x.IsTaggedWith(arg))) return false;
            return !arg.UnlocksOrder || !_selectedOrders.Any(x => x.Locked && x.OrderTagValues.Count(y => y.OrderTagGroupId == arg.Id) > 0);
        }

        private bool CanChangePrice(string arg)
        {
            return SelectedTicket != null
                && !SelectedTicket.IsLocked
                && SelectedTicket.Model.CanSubmit
                && _selectedOrders.Count == 1
                && (_selectedOrders[0].Price == 0 || _userService.IsUserPermittedFor(PermissionNames.ChangeItemPrice));
        }

        private void OnChangePrice(string obj)
        {
            decimal price;
            decimal.TryParse(AppServices.MainDataContext.NumeratorValue, out price);
            if (price <= 0)
            {
                InteractionService.UserIntraction.GiveFeedback(Resources.ForChangingPriceTypeAPrice);
            }
            else
            {
                _selectedOrders[0].UpdatePrice(price, SelectedDepartment.PriceTag);
            }
            _selectedTicket.ClearSelectedItems();
            _selectedTicket.RefreshVisuals();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetNumerator);
        }

        private bool CanExecutePrintJob(PrintJob arg)
        {
            return arg != null && SelectedTicket != null && (!SelectedTicket.IsLocked || SelectedTicket.Model.GetPrintCount(arg.Id) == 0);
        }

        private void OnPrintJobExecute(PrintJob printJob)
        {
            var message = SelectedTicket.GetPrintError();

            if (!string.IsNullOrEmpty(message))
            {
                InteractionService.UserIntraction.GiveFeedback(message);
                return;
            }

            SaveTicketIfNew();

            _ticketService.UpdateTicketNumber(SelectedTicket.Model, _applicationState.CurrentDepartment.TicketTemplate.TicketNumerator);
            _printerService.ManualPrintTicket(SelectedTicket.Model, printJob);

            if (printJob.WhenToPrint == (int)WhenToPrintTypes.Paid && !SelectedTicket.IsPaid)
                MakePaymentCommand.Execute("");
            else CloseTicket();
        }

        private void SaveTicketIfNew()
        {
            if ((SelectedTicket.Id == 0 || SelectedTicket.Orders.Any(x => x.Model.Id == 0)) && SelectedTicket.Orders.Count > 0)
            {
                var result = _ticketService.CloseTicket(SelectedTicket.Model);
                OpenTicket(result.TicketId);
            }
        }

        private bool CanRemoveTicketLock(string arg)
        {
            return SelectedTicket != null && (SelectedTicket.IsLocked) &&
                   _userService.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets);
        }

        private void OnRemoveTicketLock(string obj)
        {
            SelectedTicket.IsLocked = false;
            SelectedTicket.RefreshVisuals();
        }

        private void OnMoveOrders(string obj)
        {
            SelectedTicket.FixSelectedItems();
            var newTicketId = _ticketService.MoveOrders(SelectedTicket.Model, SelectedTicket.SelectedOrders.Select(x => x.Model), 0).TicketId;
            _selectedTicket = null;
            _selectedOrders.Clear();
            OpenTicket(newTicketId);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
            RefreshVisuals();
            SelectedTicket.ClearSelectedItems();
            SelectedTicket.PublishEvent(EventTopicNames.TicketDisplayed);
        }

        private bool CanMoveOrders(string arg)
        {
            if (SelectedTicket == null) return false;
            if (SelectedTicket.IsLocked) return false;
            if (!SelectedTicket.Model.CanRemoveSelectedOrders(SelectedTicket.SelectedOrders.Select(x => x.Model))) return false;
            if (SelectedTicket.SelectedOrders.Where(x => x.Model.Id == 0).Count() > 0) return false;
            if (SelectedTicket.SelectedOrders.Where(x => x.IsLocked).Count() == 0
                && _userService.IsUserPermittedFor(PermissionNames.MoveUnlockedOrders))
                return true;
            return _userService.IsUserPermittedFor(PermissionNames.MoveOrders);
        }

        private bool CanEditTicketNote(string arg)
        {
            return SelectedTicket != null && !SelectedTicket.IsPaid;
        }

        private void OnEditTicketNote(string obj)
        {
            SelectedTicket.Model.PublishEvent(EventTopicNames.EditTicketNote);
        }

        private void OnDecQuantityCommand(string obj)
        {
            LastSelectedOrder.Quantity--;
            _selectedTicket.RefreshVisuals();
        }

        private void OnIncQuantityCommand(string obj)
        {
            LastSelectedOrder.Quantity++;
            _selectedTicket.RefreshVisuals();
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
            _selectedTicket.RefreshVisuals();
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
            _selectedTicket.RefreshVisuals();
        }

        private bool CanCancelSelectedItems(string arg)
        {
            if (_selectedTicket != null)
                return _selectedTicket.CanCancelSelectedItems();
            return false;
        }

        private void OnCancelItemCommand(string obj)
        {
            SelectedTicket.SelectedOrders.ToList().ForEach(x => SelectedTicket.Model.CancelOrder(x.Model));
            SelectedTicket.Orders.Clear();
            SelectedTicket.Orders.AddRange(SelectedTicket.Model.Orders.Select(x => new OrderViewModel(x, SelectedDepartment.TicketTemplate, _automationService)));
            SelectedTicket.ClearSelectedItems();
            _ticketService.RecalculateTicket(SelectedTicket.Model);
            RefreshSelectedTicket();
        }

        private void OnShowAllOpenTickets(string obj)
        {
            SelectedDepartment.PublishEvent(EventTopicNames.ActivateOpenTickets);
        }

        private string _selectedTicketTitle;
        public string SelectedTicketTitle
        {
            get { return _selectedTicketTitle; }
            set { _selectedTicketTitle = value; RaisePropertyChanged(() => SelectedTicketTitle); }
        }

        public void UpdateSelectedTicketTitle()
        {
            SelectedTicketTitle = SelectedTicket == null || Totals.Title.Trim() == "#" ? Resources.NewTicket : Totals.Title;
        }

        private void OnSelectTargetAccount(string obj)
        {
            //var account = SelectedTicket.TargetAccountId == 0
            //    ? new Resource { ResourceTemplateId = SelectedTicket.TargetAccountTemplateId } :
            //    _cacheService.GetResourceById(SelectedTicket.TargetAccountId);
            //var request = new EntityOperationRequest<Resource>(account, EventTopicNames.TargetAccountSelected);
            //request.PublishEvent(EventTopicNames.SelectResource);
        }

        private void OnSelectAccountExecute(string obj)
        {
            var account = _cacheService.GetResourceById(SelectedTicket.AccountId);
            var request = new EntityOperationRequest<Resource>(account, EventTopicNames.ResourceSelected);
            request.PublishEvent(EventTopicNames.SelectResource);
        }

        private bool CanSelectAccount(string arg)
        {
            return (SelectedTicket == null ||
                (SelectedTicket.Orders.Count != 0
                && !SelectedTicket.IsLocked
                && SelectedTicket.Model.CanSubmit));
        }

        public string SelectAccountButtonCaption
        {
            get
            {
                var entityName = Totals.SourceEntityName;
                if (SelectedTicket != null && SelectedTicket.AccountId != 0)
                    return string.Format(Resources.Change_f, entityName).Replace(" ", "\r");
                return string.Format(Resources.Select_f, entityName).Replace(" ", "\r");
            }
        }

        public string SelectTargetAccountButtonCaption
        {
            get
            {
                var entityName = "";
                //if (SelectedTicket != null && SelectedTicket.TargetAccountId != 0)
                //    return string.Format(Resources.Change_f, entityName).Replace(" ", "\r");
                return string.Format(Resources.Select_f, entityName).Replace(" ", "\r");
            }
        }

        private bool CanMakePayment(string arg)
        {
            return SelectedTicket != null
                && (Totals.TicketPlainTotalValue > 0 || SelectedTicket.Orders.Count > 0)
                && _userService.IsUserPermittedFor(PermissionNames.MakePayment);
        }

        private void OnMakeFastPaymentExecute(PaymentTemplate obj)
        {
            _ticketService.PaySelectedTicket(SelectedTicket.Model, obj);
            CloseTicket();
        }

        private bool CanMakeFastPayment(PaymentTemplate arg)
        {
            return SelectedTicket != null && Totals.TicketRemainingValue > 0
                && _userService.IsUserPermittedFor(PermissionNames.MakeFastPayment);
        }

        private bool CanCloseTicket(string arg)
        {
            return SelectedTicket == null || SelectedTicket.CanCloseTicket();
        }

        public bool IsFastPaymentButtonsVisible
        {
            get
            {
                if (SelectedTicket != null && SelectedTicket.IsPaid) return false;
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.AccountName)) return false;
                if (SelectedTicket != null && SelectedTicket.IsTagged) return false;
                if (SelectedTicket != null && Totals.TicketRemainingValue == 0) return false;
                return SelectedDepartment != null && SelectedDepartment.IsFastFood;
            }
        }

        public bool IsCloseButtonVisible
        {
            get { return !IsFastPaymentButtonsVisible; }
        }

        private void OnMakePaymentExecute(string obj)
        {
            SelectedTicket.Model.PublishEvent(EventTopicNames.MakePayment);
        }

        private void OpenTicket(int id)
        {
            _applicationStateSetter.SetApplicationLocked(true);
            var ticket = _ticketService.OpenTicket(id);

            SelectedTicket = new TicketViewModel(ticket,
                _applicationState.CurrentDepartment.TicketTemplate,
                _applicationState.CurrentDepartment != null && _applicationState.CurrentDepartment.IsFastFood,
                _ticketService, _automationService, _applicationState);

            Totals.Model = ticket ?? Ticket.Empty;

            if (_applicationState.CurrentDepartment != null)
                PaymentButtonGroup.UpdatePaymentButtons(_applicationState.CurrentDepartment.TicketTemplate.PaymentTemplates.Where(x => x.DisplayUnderTicket));
        }

        private void CloseTicket()
        {
            if (_applicationState.CurrentDepartment.IsFastFood && !CanCloseTicket(""))
            {
                SaveTicketIfNew();
                RefreshVisuals();
                return;
            }
            if (!CanCloseTicket("")) return;

            if (SelectedTicket.Orders.Count > 0 && SelectedTicket.Model.GetRemainingAmount() == 0)
            {
                var message = SelectedTicket.GetPrintError();
                if (!string.IsNullOrEmpty(message))
                {
                    SelectedTicket.ClearSelectedItems();
                    RefreshVisuals();
                    InteractionService.UserIntraction.GiveFeedback(message);
                    return;
                }
            }

            SelectedTicket.ClearSelectedItems();
            var result = _ticketService.CloseTicket(SelectedTicket.Model);
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                InteractionService.UserIntraction.GiveFeedback(result.ErrorMessage);
            }

            _selectedTicket = null;
            _selectedOrders.Clear();

            if (_applicationState.CurrentTerminal.AutoLogout)
            {
                _userService.LogoutUser(false);
            }
            else EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);

            AppServices.MessagingService.SendMessage(Messages.TicketRefreshMessage, result.TicketId.ToString());
            _applicationStateSetter.SetApplicationLocked(false);
        }

        private void OnCloseTicketExecute(string obj)
        {
            CloseTicket();
        }

        private void RefreshVisuals()
        {
            UpdateSelectedTicketTitle();
            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => Totals);
            RaisePropertyChanged(() => CanDisplayAllTickets);
            RaisePropertyChanged(() => IsFastPaymentButtonsVisible);
            RaisePropertyChanged(() => IsCloseButtonVisible);
            RaisePropertyChanged(() => SelectAccountButtonCaption);
            RaisePropertyChanged(() => SelectTargetAccountButtonCaption);
            RaisePropertyChanged(() => IsLocationButtonVisible);
            RaisePropertyChanged(() => IsAccountButtonVisible);
            RaisePropertyChanged(() => IsNothingSelectedAndTicketLocked);
            RaisePropertyChanged(() => IsNothingSelectedAndTicketTagged);
            RaisePropertyChanged(() => IsTicketSelected);
            RaisePropertyChanged(() => PrintJobButtons);
            RaisePropertyChanged(() => TicketTagButtons);
        }

        private void OnAddMenuItemCommandExecute(ScreenMenuItemData obj)
        {
            if (SelectedTicket == null)
            {
                OpenTicket(0);
                RefreshVisuals();
            }

            Debug.Assert(SelectedTicket != null);

            if (SelectedTicket.IsLocked && !_userService.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets)) return;

            var ti = AddNewItem(obj.ScreenMenuItem.MenuItemId, obj.Quantity, obj.ScreenMenuItem.ItemPortion, obj.ScreenMenuItem.OrderTagTemplate);

            if (obj.ScreenMenuItem.AutoSelect && ti != null)
            {
                ti.ItemSelectedCommand.Execute(ti);
            }

            RefreshSelectedTicket();
        }
        
        public OrderViewModel AddNewItem(int menuItemId, decimal quantity, string portionName, OrderTagTemplate template)
        {
            if (!SelectedTicket.Model.CanSubmit) return null;
            SelectedTicket.ClearSelectedItems();
            var menuItem = _cacheService.GetMenuItem(x => x.Id == menuItemId);
            if (menuItem.Portions.Count == 0) return null;

            var portion = menuItem.Portions[0];

            if (!string.IsNullOrEmpty(portionName) && menuItem.Portions.Count(x => x.Name == portionName) > 0)
            {
                portion = menuItem.Portions.First(x => x.Name == portionName);
            }

            var ti = SelectedTicket.Model.AddOrder(
                _applicationState.CurrentDepartment.TicketTemplate.SaleTransactionTemplate,
                _applicationState.CurrentLoggedInUser.Name, menuItem, portion.Name, SelectedDepartment.PriceTag);

            ti.Quantity = quantity > 9 ? decimal.Round(quantity / portion.Multiplier, LocalSettings.Decimals) : quantity;

            if (template != null) template.OrderTagTemplateValues.ToList().ForEach(x => ti.ToggleOrderTag(x.OrderTagGroup, x.OrderTag, 0));

            var orderViewModel = new OrderViewModel(ti, SelectedDepartment.TicketTemplate, _automationService);
            SelectedTicket.Orders.Add(orderViewModel);
            _ticketService.RecalculateTicket(SelectedTicket.Model);
            orderViewModel.PublishEvent(EventTopicNames.OrderAdded);
            _automationService.NotifyEvent(RuleEventNames.TicketLineAdded, new { Ticket = SelectedTicket.Model, orderViewModel.Model.MenuItemName });
            return orderViewModel;
        }

        private void RefreshSelectedTicket()
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);

            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => Totals);
            RaisePropertyChanged(() => CanDisplayAllTickets);
            RaisePropertyChanged(() => IsTicketSelected);
            RaisePropertyChanged(() => IsFastPaymentButtonsVisible);
            RaisePropertyChanged(() => IsCloseButtonVisible);
        }

        public void UpdateSelectedDepartment(int departmentId)
        {
            //todo fix
            //RaisePropertyChanged(() => Departments);
            //RaisePropertyChanged(() => PermittedDepartments);
            //SelectedDepartment = departmentId > 0
            //    ? Departments.SingleOrDefault(x => x.Id == departmentId)
            //    : null;

            _applicationStateSetter.SetCurrentDepartment(departmentId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_selectedTicket != null) _selectedTicket.Dispose();
            }
        }
    }
}
