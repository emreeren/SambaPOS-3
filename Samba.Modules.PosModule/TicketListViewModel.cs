using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
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
                {
                    _selectedTicket = new TicketViewModel(_applicationState.CurrentTicket,
                        _applicationState.CurrentDepartment.TicketTemplate,
                        _applicationState.CurrentDepartment != null && _applicationState.CurrentDepartment.IsFastFood,
                        _ticketService, _automationService, _applicationState);
                    Totals = new TicketTotalsViewModel(_applicationState.CurrentTicket);
                    _ticketOrdersViewModel.SelectedTicket = _selectedTicket;
                }
                if (_selectedTicket == null && (Totals == null || Totals.Model != Ticket.Empty))
                {
                    Totals = new TicketTotalsViewModel(Ticket.Empty);
                    _ticketOrdersViewModel.SelectedTicket = null;
                }
                return _selectedTicket;
            }
        }

        public TicketTotalsViewModel Totals { get; set; }

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
        public bool IsItemsSelectedAndUnlocked { get { return _selectedOrders.Count > 0 && _selectedOrders.Where(x => x.IsLocked).Count() == 0; } }
        public bool IsItemsSelectedAndLocked { get { return _selectedOrders.Count > 0 && _selectedOrders.Where(x => !x.IsLocked).Count() == 0; } }
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
            ICacheService cacheService, TicketOrdersViewModel ticketOrdersViewModel)
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

            _selectedOrders = new ObservableCollection<OrderViewModel>();

            PrintJobCommand = new CaptionCommand<PrintJob>(Resources.Print, OnPrintJobExecute, CanExecutePrintJob);

            AddMenuItemCommand = new DelegateCommand<ScreenMenuItemData>(OnAddMenuItemCommandExecute);
            CloseTicketCommand = new CaptionCommand<string>(Resources.CloseTicket_r, OnCloseTicketExecute, CanCloseTicket);
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
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<MenuItemPortion>>().Subscribe(OnPortionSelected);
        }

        private void OnPortionSelected(EventParameters<MenuItemPortion> obj)
        {
            if (obj.Topic == EventTopicNames.PortionSelected)
            {
                var taxTemplate = _cacheService.GetMenuItem(x => x.Id == obj.Value.MenuItemId).TaxTemplate;
                SelectedOrder.UpdatePortion(obj.Value, _applicationState.CurrentDepartment.TicketTemplate.PriceTag, taxTemplate);
            }
        }

        private void OnOrderTagEvent(EventParameters<OrderTagData> obj)
        {
            if (obj.Topic == EventTopicNames.OrderTagSelected)
            {
                SelectedTicket.FixSelectedItems();
                SelectedTicket.SelectedOrders.ToList().ForEach(x =>
                    x.ToggleOrderTag(obj.Value.OrderTagGroup, obj.Value.SelectedOrderTag, _applicationState.CurrentLoggedInUser.Id));
                if (obj.Value.OrderTagGroup.IsSingleSelection)
                    SelectedTicket.ClearSelectedItems();
            }
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

            if (obj.Topic == EventTopicNames.ActivatePosView)
            {
                RefreshVisuals();
                if (SelectedTicket != null)
                    SelectedTicket.ClearSelectedItems();
            }

            if (obj.Topic == EventTopicNames.RefreshSelectedTicket)
            {
                _selectedTicket = null;
                RefreshVisuals();
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
            }
        }

        private void OnAccountSelectedFromPopup(EventParameters<PopupData> obj)
        {
            if (obj.Value.EventMessage == EventTopicNames.SelectAccount)
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
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
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
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
                }
            }
        }

        private void OnTagSelected(EventParameters<TicketTagData> obj)
        {
            if (obj.Topic == EventTopicNames.TicketTagSelected)
            {
                _ticketService.UpdateTag(obj.Value.Ticket, obj.Value.TicketTagGroup, obj.Value.SelectedTicketTag);
                SelectedTicket.ClearSelectedItems();
            }
            if (obj.Topic == EventTopicNames.TagSelectedForSelectedTicket)
            {
                if (obj.Value.TicketTagGroup.Action == 1 && CanCloseTicket(""))
                    CloseTicketCommand.Execute("");
                if (obj.Value.TicketTagGroup.Action == 2 && CanMakePayment(""))
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
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);

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
                                       SelectedOrders = _selectedOrders.Select(x => x.Model),
                                       OrderTagGroup = orderTagGroup,
                                       Ticket = SelectedTicket.Model
                                   };
            orderTagData.PublishEvent(EventTopicNames.SelectOrderTag);
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
            _selectedTicket = null;
            _selectedOrders.Clear();
            _ticketService.OpenTicket(newTicketId);
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

        private bool CanShowExtraProperty(string arg)
        {
            return SelectedOrder != null && !SelectedOrder.Model.Locked
                && _userService.IsUserPermittedFor(PermissionNames.ChangeExtraProperty);
        }

        private void OnShowExtraProperty(string obj)
        {
            _selectedTicket.Model.PublishEvent(EventTopicNames.SelectExtraProperty);
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
            if (SelectedTicket != null)
            {
                if (SelectedTicket.IsLocked || SelectedTicket.Orders.Count == 0 || (Totals.Payments.Count > 0 && !string.IsNullOrEmpty(SelectedTicket.Location)) || !SelectedTicket.Model.CanSubmit) return false;
                return string.IsNullOrEmpty(SelectedTicket.Location) || _userService.IsUserPermittedFor(PermissionNames.ChangeLocation);
            }
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
                && (Totals.TicketPlainTotalValue > 0 || SelectedTicket.Orders.Count > 0)
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
            return SelectedTicket != null && Totals.TicketRemainingValue > 0
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

        public bool IsFastPaymentButtonsVisible
        {
            get
            {
                if (SelectedTicket != null && SelectedTicket.IsPaid) return false;
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.Location)) return false;
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
            else EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);

            AppServices.MessagingService.SendMessage(Messages.TicketRefreshMessage, result.TicketId.ToString());
        }

        private void RefreshVisuals()
        {
            UpdateSelectedTicketTitle();
            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => Totals);
            RaisePropertyChanged(() => CanDisplayAllTickets);
            RaisePropertyChanged(() => IsFastPaymentButtonsVisible);
            RaisePropertyChanged(() => IsCloseButtonVisible);
            RaisePropertyChanged(() => SelectLocationButtonCaption);
            RaisePropertyChanged(() => SelectAccountButtonCaption);
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
            var menuItem = _cacheService.GetMenuItem(x => x.Id == menuItemId);
            if (menuItem.Portions.Count == 0) return null;

            var portion = menuItem.Portions[0];

            if (!string.IsNullOrEmpty(portionName) && menuItem.Portions.Count(x => x.Name == portionName) > 0)
            {
                portion = menuItem.Portions.First(x => x.Name == portionName);
            }

            var ti = SelectedTicket.Model.AddOrder(_applicationState.CurrentLoggedInUser.Name, menuItem, portion.Name, SelectedDepartment.TicketTemplate.PriceTag);

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
            //SelectedTicketView = SingleTicketView;
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
