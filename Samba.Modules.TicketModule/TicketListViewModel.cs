using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Interaction;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule
{
    [Export]
    public class TicketListViewModel : ObservableObject
    {
        private const int OpenTicketListView = 0;
        private const int SingleTicketView = 1;

        private readonly ITicketService _ticketService;
        private readonly IPrinterService _printerService;
        private readonly IAccountService _accountService;
        private readonly ILocationService _locationService;
        private readonly IUserService _userService;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IMenuService _menuService;
        private readonly IAutomationService _automationService;

        private readonly Timer _timer;

        public DelegateCommand<ScreenMenuItemData> AddMenuItemCommand { get; set; }
        public CaptionCommand<string> CloseTicketCommand { get; set; }
        public DelegateCommand<int?> OpenTicketCommand { get; set; }
        public CaptionCommand<string> MakePaymentCommand { get; set; }

        public CaptionCommand<string> MakeCashPaymentCommand { get; set; }
        public CaptionCommand<string> MakeCreditCardPaymentCommand { get; set; }
        public CaptionCommand<string> MakeTicketPaymentCommand { get; set; }
        public CaptionCommand<string> SelectLocationCommand { get; set; }
        public CaptionCommand<string> SelectAccountCommand { get; set; }
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
        public ICaptionCommand ShowExtraPropertyEditorCommand { get; set; }
        public ICaptionCommand EditTicketNoteCommand { get; set; }
        public ICaptionCommand RemoveTicketLockCommand { get; set; }
        public ICaptionCommand RemoveTicketTagCommand { get; set; }
        public ICaptionCommand ChangePriceCommand { get; set; }
        public ICaptionCommand PrintJobCommand { get; set; }

        private TicketViewModel _selectedTicket;
        public TicketViewModel SelectedTicket
        {
            get
            {
                if (_applicationState.CurrentTicket == null) _selectedTicket = null;
                if (_selectedTicket == null && _applicationState.CurrentTicket != null)
                    _selectedTicket = new TicketViewModel(_applicationState.CurrentTicket, _applicationState.CurrentDepartment.TicketTemplate,
                      _applicationState.CurrentDepartment != null && _applicationState.CurrentDepartment.IsFastFood, _ticketService, _userService, _menuService, _automationService, _applicationState);
                return _selectedTicket;
            }
        }

        private readonly ObservableCollection<OrderViewModel> _selectedOrders;
        public OrderViewModel SelectedOrder
        {
            get
            {
                return SelectedTicket != null && SelectedTicket.SelectedOrders.Count == 1 ? SelectedTicket.SelectedOrders[0] : null;
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

        public IEnumerable<OpenTicketViewModel> OpenTickets { get; set; }

        private int _selectedTicketView;
        public int SelectedTicketView
        {
            get { return _selectedTicketView; }
            set
            {
                StopTimer();
                if (value == OpenTicketListView)
                {
                    _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.TicketList);
                    StartTimer();
                }
                if (value == SingleTicketView)
                {
                    _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.SingleTicket);
                }
                _selectedTicketView = value;
                RaisePropertyChanged(() => SelectedTicketView);
            }
        }

        public Department SelectedDepartment
        {
            get { return _applicationState.CurrentDepartment; }
            set
            {
                if (value != _applicationState.CurrentDepartment)
                {
                    _applicationStateSetter.SetCurrentDepartment(value);
                    RaisePropertyChanged(() => SelectedDepartment);
                    RaisePropertyChanged(() => SelectedTicket);
                    SelectedDepartment.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
                }
            }
        }

        public bool CanDisplayAllTickets { get { return SelectedTicket == null; } }
        public bool IsItemsSelected { get { return _selectedOrders.Count > 0; } }
        public bool IsItemsSelectedAndUnlocked { get { return _selectedOrders.Count > 0 && _selectedOrders.Where(x => x.IsLocked).Count() == 0; } }
        public bool IsItemsSelectedAndLocked { get { return _selectedOrders.Count > 0 && _selectedOrders.Where(x => !x.IsLocked).Count() == 0; } }
        public bool IsNothingSelected { get { return _selectedOrders.Count == 0; } }
        public bool IsNothingSelectedAndTicketLocked { get { return _selectedTicket != null && _selectedOrders.Count == 0 && _selectedTicket.IsLocked; } }
        public bool IsNothingSelectedAndTicketTagged { get { return _selectedTicket != null && _selectedOrders.Count == 0 && SelectedTicket.IsTagged; } }
        public bool IsTicketSelected { get { return SelectedTicket != null && _selectedOrders.Count == 0; } }
        public bool IsTicketTotalVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketTotalVisible; } }
        public bool IsTicketPaymentVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketPaymentVisible; } }
        public bool IsTicketRemainingVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketRemainingVisible; } }
        public bool IsTicketDiscountVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketDiscountVisible; } }
        public bool IsTicketRoundingVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketRoundingVisible; } }
        public bool IsTicketTaxTotalVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketTaxTotalVisible; } }
        public bool IsTicketServiceVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketServiceVisible; } }
        public bool IsPlainTotalVisible { get { return IsTicketDiscountVisible || IsTicketTaxTotalVisible || IsTicketRoundingVisible || IsTicketServiceVisible; } }

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

        public Brush TicketBackground { get { return SelectedTicket != null && (SelectedTicket.IsLocked || SelectedTicket.IsPaid) ? SystemColors.ControlLightBrush : SystemColors.WindowBrush; } }

        public int OpenTicketListViewColumnCount { get { return SelectedDepartment != null ? SelectedDepartment.OpenTicketViewColumnCount : 5; } }

        public OrderViewModel LastSelectedOrder { get; set; }

        public IEnumerable<TicketTagButton> TicketTagButtons
        {
            get
            {
                return _applicationState.CurrentDepartment != null
                    ? _applicationState.CurrentDepartment.TicketTemplate.TicketTagGroups
                    .Where(x => x.ActiveOnPosClient)
                    .OrderBy(x => x.Order)
                    .Select(x => new TicketTagButton(x, SelectedTicket.Model))
                    : null;
            }
        }

        public IEnumerable<OrderTagButton> OrderTagButtons
        {
            get
            {
                if (_selectedOrders != null && _selectedOrders.Count > 0)
                {
                    return _ticketService.GetOrderTagGroupsForItems(_selectedOrders.Select(x => x.MenuItem))
                        .Where(x => !string.IsNullOrEmpty(x.ButtonHeader))
                        .Select(x => new OrderTagButton(x));
                }
                return null;
            }
        }

        [ImportingConstructor]
        public TicketListViewModel(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ITicketService ticketService, IAccountService accountService, IPrinterService printerService,
            ILocationService locationService, IUserService userService, IMenuService menuService, IAutomationService automationService)
        {
            _printerService = printerService;
            _ticketService = ticketService;
            _accountService = accountService;
            _locationService = locationService;
            _userService = userService;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _menuService = menuService;
            _automationService = automationService;

            _timer = new Timer(OnTimer, null, Timeout.Infinite, 1000);
            _selectedOrders = new ObservableCollection<OrderViewModel>();

            PrintJobCommand = new CaptionCommand<PrintJob>(Resources.Print, OnPrintJobExecute, CanExecutePrintJob);

            AddMenuItemCommand = new DelegateCommand<ScreenMenuItemData>(OnAddMenuItemCommandExecute);
            CloseTicketCommand = new CaptionCommand<string>(Resources.CloseTicket_r, OnCloseTicketExecute, CanCloseTicket);
            OpenTicketCommand = new DelegateCommand<int?>(OnOpenTicketExecute);
            MakePaymentCommand = new CaptionCommand<string>(Resources.GetPayment, OnMakePaymentExecute, CanMakePayment);
            MakeCashPaymentCommand = new CaptionCommand<string>(Resources.CashPayment_r, OnMakeCashPaymentExecute, CanMakeFastPayment);
            MakeCreditCardPaymentCommand = new CaptionCommand<string>(Resources.CreditCard_r, OnMakeCreditCardPaymentExecute, CanMakeFastPayment);
            MakeTicketPaymentCommand = new CaptionCommand<string>(Resources.Voucher_r, OnMakeTicketPaymentExecute, CanMakeFastPayment);
            SelectLocationCommand = new CaptionCommand<string>(Resources.SelectLocation, OnSelectLocationExecute, CanSelectLocation);
            SelectAccountCommand = new CaptionCommand<string>(Resources.SelectAccount, OnSelectAccountExecute, CanSelectAccount);
            ShowAllOpenTickets = new CaptionCommand<string>(Resources.AllTickets_r, OnShowAllOpenTickets);

            IncQuantityCommand = new CaptionCommand<string>("+", OnIncQuantityCommand, CanIncQuantity);
            DecQuantityCommand = new CaptionCommand<string>("-", OnDecQuantityCommand, CanDecQuantity);
            IncSelectionQuantityCommand = new CaptionCommand<string>("(+)", OnIncSelectionQuantityCommand, CanIncSelectionQuantity);
            DecSelectionQuantityCommand = new CaptionCommand<string>("(-)", OnDecSelectionQuantityCommand, CanDecSelectionQuantity);
            ShowTicketTagsCommand = new CaptionCommand<TicketTagGroup>(Resources.Tag, OnShowTicketsTagExecute, CanExecuteShowTicketTags);
            ShowOrderTagsCommand = new CaptionCommand<OrderTagGroup>(Resources.Tag, OnShowOrderTagsExecute, CanShowOrderTagsExecute);
            CancelItemCommand = new CaptionCommand<string>(Resources.Cancel, OnCancelItemCommand, CanCancelSelectedItems);
            MoveOrdersCommand = new CaptionCommand<string>(Resources.MoveTicketLine, OnMoveOrders, CanMoveOrders);
            ShowExtraPropertyEditorCommand = new CaptionCommand<string>(Resources.ExtraModifier, OnShowExtraProperty, CanShowExtraProperty);
            EditTicketNoteCommand = new CaptionCommand<string>(Resources.TicketNote, OnEditTicketNote, CanEditTicketNote);
            RemoveTicketLockCommand = new CaptionCommand<string>(Resources.ReleaseLock, OnRemoveTicketLock, CanRemoveTicketLock);
            ChangePriceCommand = new CaptionCommand<string>(Resources.ChangePrice, OnChangePrice, CanChangePrice);

            EventServiceFactory.EventService.GetEvent<GenericEvent<ScreenMenuItemData>>().Subscribe(OnMenuItemSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<LocationData>>().Subscribe(OnLocationSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderViewModel>>().Subscribe(OnSelectedOrdersChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagData>>().Subscribe(OnTagSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketViewModel>>().Subscribe(OnTicketSelectedOrdersChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(OnAccountSelectedForTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnPaymentSubmitted);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnRefreshTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(OnMessageReceived);
            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(OnAccountSelectedFromPopup);
        }

        private void OnTicketSelectedOrdersChanged(EventParameters<TicketViewModel> obj)
        {
            if (obj.Topic == EventTopicNames.SelectedOrdersChanged)
            {
                _selectedOrders.Clear();
                _selectedOrders.AddRange(obj.Value.SelectedOrders);
                if (obj.Value.SelectedOrders.Count == 0) LastSelectedOrder = null;
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
        }

        private void OnRefreshTicket(EventParameters<EventAggregator> obj)
        {
            if (SelectedDepartment == null && _applicationState.CurrentLoggedInUser.UserRole.DepartmentId > 0)
                UpdateSelectedDepartment(_applicationState.CurrentLoggedInUser.UserRole.DepartmentId);

            if (obj.Topic == EventTopicNames.ActivateTicketView)
            {
                UpdateSelectedTicketView();
                DisplayTickets();
            }

            if (obj.Topic == EventTopicNames.DisplayTicketView)
            {
                UpdateSelectedTicketView();
                RefreshVisuals();
            }

            if (obj.Topic == EventTopicNames.RefreshSelectedTicket)
            {
                _selectedTicket = null;
                RefreshVisuals();
                SelectedTicketView = SingleTicketView;
            }
        }

        private void OnAccountSelectedFromPopup(EventParameters<PopupData> obj)
        {
            if (obj.Value.EventMessage == EventTopicNames.SelectAccount)
            {
                //todo fix
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
                UpdateOpenTickets(SelectedDepartment);
                RefreshVisuals();
            }
        }

        private void OnPaymentSubmitted(EventParameters<Ticket> obj)
        {
            _selectedTicket = null;
            if (obj.Topic == EventTopicNames.PaymentSubmitted)
                CloseTicket();
        }

        private void OnAccountSelectedForTicket(EventParameters<Account> obj)
        {
            if (obj.Topic == EventTopicNames.AccountSelectedForTicket)
            {
                _ticketService.UpdateAccount(SelectedTicket.Model, obj.Value);

                if (!string.IsNullOrEmpty(SelectedTicket.AccountName) && SelectedTicket.Orders.Count > 0)
                    CloseTicket();
                else
                {
                    RefreshVisuals();
                    SelectedTicketView = SingleTicketView;
                }
            }

            if (obj.Topic == EventTopicNames.PaymentRequestedForTicket)
            {
                _ticketService.UpdateAccount(SelectedTicket.Model, obj.Value);
                if (!string.IsNullOrEmpty(SelectedTicket.AccountName) && SelectedTicket.Orders.Count > 0)
                    MakePaymentCommand.Execute("");
                else
                {
                    RefreshVisuals();
                    SelectedTicketView = SingleTicketView;
                }
            }
        }

        private void OnTagSelected(EventParameters<TicketTagData> obj)
        {
            if (obj.Topic == EventTopicNames.TagSelectedForSelectedTicket)
            {
                if (obj.Value.Action == 1 && CanCloseTicket(""))
                    CloseTicketCommand.Execute("");
                if (obj.Value.Action == 2 && CanMakePayment(""))
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
                { item.IsLastSelected = item == LastSelectedOrder; }
                SelectedTicket.PublishEvent(EventTopicNames.SelectedOrdersChanged);
            }
        }

        private void OnMenuItemSelected(EventParameters<ScreenMenuItemData> obj)
        {
            if (obj.Topic == EventTopicNames.ScreenMenuItemDataSelected) AddMenuItemCommand.Execute(obj.Value);
        }

        private void OnLocationSelected(EventParameters<LocationData> obj)
        {
            if (obj.Topic == EventTopicNames.LocationSelectedForTicket)
            {
                if (SelectedTicket != null)
                {
                    var oldLocationName = SelectedTicket.Location;
                    var ticketsMerged = obj.Value.TicketId > 0 && obj.Value.TicketId != SelectedTicket.Id;
                    _ticketService.ChangeTicketLocation(SelectedTicket.Model, obj.Value.LocationId);

                    CloseTicket();

                    if (!_applicationState.CurrentTerminal.AutoLogout)
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicketView);

                    if (!string.IsNullOrEmpty(oldLocationName) || ticketsMerged)
                        if (ticketsMerged && !string.IsNullOrEmpty(oldLocationName))
                            InteractionService.UserIntraction.GiveFeedback(string.Format(Resources.LocationsMerged_f, oldLocationName, obj.Value.Caption));
                        else if (ticketsMerged)
                            InteractionService.UserIntraction.GiveFeedback(string.Format(Resources.TicketMergedToLocation_f, obj.Value.Caption));
                        else if (oldLocationName != obj.Value.LocationName)
                            InteractionService.UserIntraction.GiveFeedback(string.Format(Resources.TicketMovedToLocation_f, oldLocationName, obj.Value.Caption));
                }
                else
                {
                    if (obj.Value.TicketId == 0)
                    {
                        _ticketService.OpenTicket(0);
                        if (SelectedTicket != null)
                        {
                            _ticketService.ChangeTicketLocation(SelectedTicket.Model, obj.Value.LocationId);
                        }
                    }
                    else
                    {
                        _ticketService.OpenTicket(obj.Value.TicketId);
                        if (SelectedTicket != null)
                        {
                            if (SelectedTicket.Location != obj.Value.LocationName)
                                _ticketService.ResetLocationData(_applicationState.CurrentTicket);
                        }
                    }
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.DisplayTicketView);
                }
            }
        }

        private void UpdateSelectedTicketView()
        {
            if (SelectedTicket != null || (SelectedDepartment != null && SelectedDepartment.IsFastFood))
                SelectedTicketView = SingleTicketView;
            else
            {
                SelectedTicketView = OpenTicketListView;
                UpdateOpenTickets(SelectedDepartment);
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
                _selectedTicket.LastSelectedTicketTag = tagGroup;
                _selectedTicket.PublishEvent(EventTopicNames.SelectTicketTag);
            }
            else if (ShowAllOpenTickets.CanExecute(""))
            {
                UpdateOpenTickets(SelectedDepartment);
                if ((OpenTickets != null && OpenTickets.Count() > 0))
                {
                    SelectedTicketView = OpenTicketListView;
                    RaisePropertyChanged(() => OpenTickets);
                }
                else InteractionService.UserIntraction.GiveFeedback(string.Format(Resources.NoTicketsFoundForTag, tagGroup.Name));
            }
        }

        private void OnShowOrderTagsExecute(OrderTagGroup orderTagGroup)
        {
            _selectedTicket.LastSelectedOrderTag = orderTagGroup;
            _selectedTicket.PublishEvent(EventTopicNames.SelectOrderTag);
        }

        private bool CanShowOrderTagsExecute(OrderTagGroup arg)
        {
            if (_selectedOrders.Count == 0) return false;
            if (!arg.DecreaseOrderInventory && _selectedOrders.Any(x => !x.IsLocked && !x.IsTaggedWith(arg))) return false;
            if (_selectedOrders.Any(x => !x.Model.DecreaseInventory && !x.IsTaggedWith(arg))) return false;
            return !arg.UnlocksOrder || !_selectedOrders.Any(x => x.IsLocked && x.OrderTagValues.Count(y => y.Model.OrderTagGroupId == arg.Id) > 0);
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
                _selectedOrders[0].UpdatePrice(price);
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
                _ticketService.OpenTicket(result.TicketId);
                _selectedTicket = null;
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
            OnOpenTicketExecute(newTicketId);
        }

        private bool CanMoveOrders(string arg)
        {
            return SelectedTicket != null && SelectedTicket.CanMoveSelectedOrders();
        }

        private bool CanEditTicketNote(string arg)
        {
            return SelectedTicket != null && !SelectedTicket.IsPaid;
        }

        private void OnEditTicketNote(string obj)
        {
            SelectedTicket.PublishEvent(EventTopicNames.EditTicketNote);
        }

        private bool CanShowExtraProperty(string arg)
        {
            return SelectedOrder != null && !SelectedOrder.Model.Locked
                && _userService.IsUserPermittedFor(PermissionNames.ChangeExtraProperty);
        }

        private void OnShowExtraProperty(string obj)
        {
            _selectedTicket.PublishEvent(EventTopicNames.SelectExtraProperty);
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
            SelectedTicket.Orders.AddRange(SelectedTicket.Model.Orders.Select(x => new OrderViewModel(x, SelectedDepartment.TicketTemplate, _menuService, _automationService)));
            SelectedTicket.ClearSelectedItems();
            _ticketService.RecalculateTicket(SelectedTicket.Model);
            RefreshSelectedTicket();
        }

        private void OnTimer(object state)
        {
            if (_applicationState.ActiveAppScreen == AppScreens.TicketList && OpenTickets != null)
                foreach (var openTicketView in OpenTickets)
                {
                    openTicketView.Refresh();
                }
        }

        private void OnShowAllOpenTickets(string obj)
        {
            UpdateOpenTickets(null);
            SelectedTicketView = OpenTicketListView;
            RaisePropertyChanged(() => OpenTickets);
        }

        private string _selectedTicketTitle;
        public string SelectedTicketTitle
        {
            get { return _selectedTicketTitle; }
            set { _selectedTicketTitle = value; RaisePropertyChanged(() => SelectedTicketTitle); }
        }

        public void UpdateSelectedTicketTitle()
        {
            SelectedTicketTitle = SelectedTicket == null || SelectedTicket.Title.Trim() == "#" ? Resources.NewTicket : SelectedTicket.Title;
        }

        private void OnSelectAccountExecute(string obj)
        {
            SelectedDepartment.PublishEvent(EventTopicNames.SelectAccount);
        }

        private bool CanSelectAccount(string arg)
        {
            return (SelectedTicket == null ||
                (SelectedTicket.Orders.Count != 0
                && !SelectedTicket.IsLocked
                && SelectedTicket.Model.CanSubmit));
        }

        private bool CanSelectLocation(string arg)
        {
            if (SelectedTicket != null && !SelectedTicket.IsLocked)
                return SelectedTicket.CanChangeLocation();
            return SelectedTicket == null;
        }

        private void OnSelectLocationExecute(string obj)
        {
            SelectedDepartment.PublishEvent(EventTopicNames.SelectLocation);
        }

        public string SelectLocationButtonCaption
        {
            get
            {
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.Location))
                    return Resources.ChangeLocation_r;
                return Resources.SelectLocation_r;
            }
        }

        public string SelectAccountButtonCaption
        {
            get
            {
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.AccountName))
                    return Resources.AccountInfo_r;
                return Resources.SelectAccount_r;
            }
        }

        private bool CanMakePayment(string arg)
        {
            return SelectedTicket != null
                && (SelectedTicket.TicketPlainTotalValue > 0 || SelectedTicket.Orders.Count > 0)
                && _userService.IsUserPermittedFor(PermissionNames.MakePayment);
        }

        private void OnMakeCreditCardPaymentExecute(string obj)
        {
            _ticketService.PaySelectedTicket(SelectedTicket.Model, PaymentType.CreditCard);
            CloseTicket();
        }

        private void OnMakeTicketPaymentExecute(string obj)
        {
            _ticketService.PaySelectedTicket(SelectedTicket.Model, PaymentType.Ticket);
            CloseTicket();
        }

        private void OnMakeCashPaymentExecute(string obj)
        {
            _ticketService.PaySelectedTicket(SelectedTicket.Model, PaymentType.Ticket);
            CloseTicket();
        }

        private bool CanMakeFastPayment(string arg)
        {
            return SelectedTicket != null && SelectedTicket.TicketRemainingValue > 0
                && _userService.IsUserPermittedFor(PermissionNames.MakeFastPayment);
        }

        private bool CanCloseTicket(string arg)
        {
            return SelectedTicket == null || SelectedTicket.CanCloseTicket();
        }

        private void CloseTicket()
        {
            if (_applicationState.CurrentDepartment.IsFastFood && !CanCloseTicket(""))
            {
                SaveTicketIfNew();
                RefreshVisuals();
            }
            else if (CanCloseTicket(""))
                CloseTicketCommand.Execute("");
        }

        public void DisplayTickets()
        {
            if (SelectedDepartment != null)
            {
                if (SelectedDepartment.IsAlaCarte && SelectedDepartment.LocationScreens.Count > 0)
                {
                    SelectedDepartment.PublishEvent(EventTopicNames.SelectLocation);
                    StopTimer();
                    RefreshVisuals();
                    return;
                }

                if (SelectedDepartment.IsTakeAway)
                {
                    SelectedDepartment.PublishEvent(EventTopicNames.SelectAccount);
                    StopTimer();
                    RefreshVisuals();
                    return;
                }

                SelectedTicketView = SelectedDepartment.IsFastFood ? SingleTicketView : OpenTicketListView;

                if (SelectedTicket != null)
                {
                    if (!SelectedDepartment.IsFastFood || SelectedTicket.TicketRemainingValue == 0 || !string.IsNullOrEmpty(SelectedTicket.Location))
                    {
                        SelectedTicket.ClearSelectedItems();
                    }
                }
            }
            UpdateOpenTickets(SelectedDepartment);
            RefreshVisuals();

        }

        public bool IsFastPaymentButtonsVisible
        {
            get
            {
                if (SelectedTicket != null && SelectedTicket.IsPaid) return false;
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.Location)) return false;
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.AccountName)) return false;
                if (SelectedTicket != null && SelectedTicket.IsTagged) return false;
                if (SelectedTicket != null && SelectedTicket.TicketRemainingValue == 0) return false;
                return SelectedDepartment != null && SelectedDepartment.IsFastFood;
            }
        }

        public bool IsCloseButtonVisible
        {
            get { return !IsFastPaymentButtonsVisible; }
        }

        public void UpdateOpenTickets(Department department)
        {
            StopTimer();

            Expression<Func<Ticket, bool>> prediction;

            if (department != null)
                prediction = x => !x.IsPaid && x.DepartmentId == department.Id;
            else
                prediction = x => !x.IsPaid;

            var shouldWrap = !SelectedDepartment.IsTakeAway;

            OpenTickets = Dao.Select(x => new OpenTicketViewModel
            {
                Id = x.Id,
                LastOrderDate = x.LastOrderDate,
                TicketNumber = x.TicketNumber,
                LocationName = x.LocationName,
                AccountName = x.AccountName,
                RemainingAmount = x.RemainingAmount,
                Date = x.Date,
                WrapText = shouldWrap
            }, prediction).OrderBy(x => x.LastOrderDate);

            StartTimer();
        }

        private void StartTimer()
        {
            if (_applicationState.ActiveAppScreen == AppScreens.TicketList)
                _timer.Change(60000, 60000);
        }

        private void StopTimer()
        {
            _timer.Change(Timeout.Infinite, 60000);
        }

        private void OnMakePaymentExecute(string obj)
        {
            _applicationState.CurrentTicket.PublishEvent(EventTopicNames.MakePayment);
        }

        private void OnCloseTicketExecute(string obj)
        {
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

            _automationService.NotifyEvent(RuleEventNames.TicketClosed, new { Ticket = _selectedTicket.Model });

            _selectedTicket = null;
            _selectedOrders.Clear();

            if (_applicationState.CurrentTerminal.AutoLogout)
            {
                _userService.LogoutUser(false);
            }
            else
            {
                DisplayTickets();
            }
            AppServices.MessagingService.SendMessage(Messages.TicketRefreshMessage, result.TicketId.ToString());
        }

        private void OnOpenTicketExecute(int? id)
        {
            _selectedTicket = null;
            _selectedOrders.Clear();
            _ticketService.OpenTicket(id.GetValueOrDefault(0));
            SelectedTicketView = SingleTicketView;
            RefreshVisuals();
            SelectedTicket.ClearSelectedItems();
            SelectedTicket.PublishEvent(EventTopicNames.TicketDisplayed);
        }

        private void RefreshVisuals()
        {
            UpdateSelectedTicketTitle();
            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => CanDisplayAllTickets);
            RaisePropertyChanged(() => IsTicketRemainingVisible);
            RaisePropertyChanged(() => IsTicketPaymentVisible);
            RaisePropertyChanged(() => IsTicketTotalVisible);
            RaisePropertyChanged(() => IsTicketDiscountVisible);
            RaisePropertyChanged(() => IsTicketTaxTotalVisible);
            RaisePropertyChanged(() => IsTicketServiceVisible);
            RaisePropertyChanged(() => IsTicketRoundingVisible);
            RaisePropertyChanged(() => IsPlainTotalVisible);
            RaisePropertyChanged(() => IsFastPaymentButtonsVisible);
            RaisePropertyChanged(() => IsCloseButtonVisible);
            RaisePropertyChanged(() => SelectLocationButtonCaption);
            RaisePropertyChanged(() => SelectAccountButtonCaption);
            RaisePropertyChanged(() => OpenTicketListViewColumnCount);
            RaisePropertyChanged(() => TicketBackground);
            RaisePropertyChanged(() => IsLocationButtonVisible);
            RaisePropertyChanged(() => IsAccountButtonVisible);
            RaisePropertyChanged(() => IsNothingSelectedAndTicketLocked);
            RaisePropertyChanged(() => IsNothingSelectedAndTicketTagged);
            RaisePropertyChanged(() => IsTicketSelected);
            RaisePropertyChanged(() => PrintJobButtons);
            RaisePropertyChanged(() => TicketTagButtons);

            if (SelectedTicketView == OpenTicketListView)
                RaisePropertyChanged(() => OpenTickets);
        }

        private void OnAddMenuItemCommandExecute(ScreenMenuItemData obj)
        {
            if (SelectedTicket == null)
            {
                _ticketService.OpenTicket(0);
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
            var menuItem = _menuService.GetMenuItem(menuItemId);
            if (menuItem.Portions.Count == 0) return null;

            var portion = menuItem.Portions[0];

            if (!string.IsNullOrEmpty(portionName) && menuItem.Portions.Count(x => x.Name == portionName) > 0)
            {
                portion = menuItem.Portions.First(x => x.Name == portionName);
            }

            var ti = SelectedTicket.Model.AddOrder(_applicationState.CurrentLoggedInUser.Name, menuItem, portion.Name, SelectedDepartment.TicketTemplate.PriceTag);

            ti.Quantity = quantity > 9 ? decimal.Round(quantity / portion.Multiplier, LocalSettings.Decimals) : quantity;

            if (template != null) template.OrderTagTemplateValues.ToList().ForEach(x => ti.ToggleOrderTag(x.OrderTagGroup, x.OrderTag, 0));

            var orderViewModel = new OrderViewModel(ti, SelectedDepartment.TicketTemplate, _menuService, _automationService);
            SelectedTicket.Orders.Add(orderViewModel);
            _ticketService.RecalculateTicket(SelectedTicket.Model);
            orderViewModel.PublishEvent(EventTopicNames.OrderAdded);
            _automationService.NotifyEvent(RuleEventNames.TicketLineAdded, new { Ticket = SelectedTicket.Model, orderViewModel.Model.MenuItemName });
            return orderViewModel;
        }

        private void RefreshSelectedTicket()
        {
            SelectedTicketView = SingleTicketView;

            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => IsTicketRemainingVisible);
            RaisePropertyChanged(() => IsTicketPaymentVisible);
            RaisePropertyChanged(() => IsTicketTotalVisible);
            RaisePropertyChanged(() => IsTicketDiscountVisible);
            RaisePropertyChanged(() => IsTicketTaxTotalVisible);
            RaisePropertyChanged(() => IsTicketServiceVisible);
            RaisePropertyChanged(() => IsTicketRoundingVisible);
            RaisePropertyChanged(() => IsPlainTotalVisible);
            RaisePropertyChanged(() => CanDisplayAllTickets);
            RaisePropertyChanged(() => TicketBackground);
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
                if (_timer != null) _timer.Dispose();
                if (_selectedTicket != null) _selectedTicket.Dispose();
            }
        }
    }
}
