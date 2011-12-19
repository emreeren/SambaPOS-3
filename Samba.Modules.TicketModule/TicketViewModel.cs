using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class TicketViewModel : ObservableObject
    {
        private readonly Ticket _model;
        private readonly TicketTemplate _ticketTemplate;
        private readonly bool _forcePayment;
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly IMenuService _menuService;

        public TicketViewModel(Ticket model, TicketTemplate ticketTemplate, bool forcePayment,
            ITicketService ticketService, IUserService userService, IMenuService menuService)
        {
            _ticketService = ticketService;
            _userService = userService;
            _forcePayment = forcePayment;
            _model = model;
            _ticketTemplate = ticketTemplate;
            _menuService = menuService;

            _orders = new ObservableCollection<OrderViewModel>(model.Orders.Select(x => new OrderViewModel(x, ticketTemplate, _menuService)).OrderBy(x => x.Model.CreatedDateTime));
            _payments = new ObservableCollection<PaymentViewModel>(model.Payments.Select(x => new PaymentViewModel(x)));
            _discounts = new ObservableCollection<DiscountViewModel>(model.Discounts.Select(x => new DiscountViewModel(x)));

            _itemsViewSource = new CollectionViewSource { Source = _orders };
            _itemsViewSource.GroupDescriptions.Add(new PropertyGroupDescription("GroupObject"));

            SelectAllItemsCommand = new CaptionCommand<string>("", OnSelectAllItemsExecute);

            PrintJobButtons = AppServices.CurrentTerminal.PrintJobs
                .Where(x => (!string.IsNullOrEmpty(x.ButtonHeader))
                    && (x.PrinterMaps.Count(y => y.Department == null || y.Department.Id == model.DepartmentId) > 0))
                .OrderBy(x => x.Order)
                .Select(x => new PrintJobButton(x, Model));

            if (PrintJobButtons.Count(x => x.Model.UseForPaidTickets) > 0)
            {
                PrintJobButtons = IsPaid
                    ? PrintJobButtons.Where(x => x.Model.UseForPaidTickets)
                    : PrintJobButtons.Where(x => !x.Model.UseForPaidTickets);
            }
        }

        private void OnSelectAllItemsExecute(string obj)
        {
            foreach (var order in Orders.Where(x => x.OrderNumber == obj))
                order.ToggleSelection();

            RefreshVisuals();

            this.PublishEvent(EventTopicNames.SelectedOrdersChanged);

        }

        public Ticket Model
        {
            get { return _model; }
        }

        private readonly ObservableCollection<OrderViewModel> _orders;
        public ObservableCollection<OrderViewModel> Orders
        {
            get { return _orders; }
        }

        private CollectionViewSource _itemsViewSource;
        public CollectionViewSource ItemsViewSource
        {
            get { return _itemsViewSource; }
            set { _itemsViewSource = value; }
        }

        private readonly ObservableCollection<PaymentViewModel> _payments;
        public ObservableCollection<PaymentViewModel> Payments
        {
            get { return _payments; }
        }

        private readonly ObservableCollection<DiscountViewModel> _discounts;
        public ObservableCollection<DiscountViewModel> Discounts
        {
            get { return _discounts; }
        }

        public ObservableCollection<OrderViewModel> SelectedOrders
        {
            get { return new ObservableCollection<OrderViewModel>(Orders.Where(x => x.Selected)); }
        }

        public IEnumerable<PrintJobButton> PrintJobButtons { get; set; }

        public ICaptionCommand SelectAllItemsCommand { get; set; }

        public DateTime Date
        {
            get { return Model.Date; }
            set { Model.Date = value; }
        }

        public int Id
        {
            get { return Model.Id; }
        }

        public string Note
        {
            get { return Model.Note; }
            set { Model.Note = value; RaisePropertyChanged(() => Note); }
        }

        public string TagDisplay { get { return Model.GetTagData().Split('\r').Select(x => !string.IsNullOrEmpty(x) && x.Contains(":") && x.Split(':')[0].Trim() == x.Split(':')[1].Trim() ? x.Split(':')[0] : x).Aggregate("", (c, v) => c + v + "\r").Trim('\r'); } }

        public bool IsTicketNoteVisible { get { return !string.IsNullOrEmpty(Note); } }

        public bool IsPaid { get { return Model.IsPaid; } }

        public decimal TicketTotalValue
        {
            get { return Model.GetSum(); }
        }

        public decimal TicketTaxValue
        {
            get { return Model.CalculateTax(); }
        }

        public decimal TicketServiceValue
        {
            get { return Model.GetServicesTotal(); }
        }

        public decimal TicketPaymentValue
        {
            get { return Model.GetPaymentAmount(); }
        }

        public decimal TicketRemainingValue
        {
            get { return Model.GetRemainingAmount(); }
        }

        public decimal TicketPlainTotalValue
        {
            get { return Model.GetPlainSum(); }
        }

        public string TicketPlainTotalLabel
        {
            get { return TicketPlainTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketTotalLabel
        {
            get { return TicketTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public decimal TicketDiscountAmount
        {
            get { return Model.GetDiscountTotal(); }
        }

        public string TicketDiscountLabel
        {
            get { return TicketDiscountAmount.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public decimal TicketRoundingAmount
        {
            get { return Model.GetRoundingTotal(); }
        }

        public string TicketRoundingLabel
        {
            get { return TicketRoundingAmount.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketTaxLabel
        {
            get { return TicketTaxValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketServiceLabel
        {
            get { return TicketServiceValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketPaymentLabel
        {
            get { return TicketPaymentValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketRemainingLabel
        {
            get { return TicketRemainingValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketCreationDate
        {
            get
            {
                if (IsPaid) return Model.Date.ToString();
                var time = new TimeSpan(DateTime.Now.Ticks - Model.Date.Ticks).TotalMinutes.ToString("#");

                return !string.IsNullOrEmpty(time)
                    ? string.Format(Resources.TicketTimeDisplay_f, Model.Date.ToShortTimeString(), time)
                    : Model.Date.ToShortTimeString();
            }
        }

        public string TicketLastOrderDate
        {
            get
            {
                if (IsPaid) return Model.LastOrderDate.ToString();
                var time = new TimeSpan(DateTime.Now.Ticks - Model.LastOrderDate.Ticks).TotalMinutes.ToString("#");
                return !string.IsNullOrEmpty(time)
                    ? string.Format(Resources.TicketTimeDisplay_f, Model.LastOrderDate.ToShortTimeString(), time)
                    : Model.LastOrderDate.ToShortTimeString();
            }
        }

        public string TicketLastPaymentDate
        {
            get
            {
                if (!IsPaid) return Model.LastPaymentDate != Model.Date ? Model.LastPaymentDate.ToShortTimeString() : "-";
                var time = new TimeSpan(Model.LastPaymentDate.Ticks - Model.Date.Ticks).TotalMinutes.ToString("#");
                return !string.IsNullOrEmpty(time)
                    ? string.Format(Resources.TicketTimeDisplay_f, Model.LastPaymentDate, time)
                    : Model.LastPaymentDate.ToString();
            }
        }

        public bool IsTicketTimeVisible { get { return Model.Id != 0; } }
        public bool IsLastPaymentDateVisible { get { return Model.Payments.Count > 0; } }
        public bool IsLastOrderDateVisible
        {
            get
            {
                return Model.Orders.Count > 1 && Model.Orders[Model.Orders.Count - 1].OrderNumber != 0 &&
                    Model.Orders[0].OrderNumber != Model.Orders[Model.Orders.Count - 1].OrderNumber;
            }
        }

        public void ClearSelectedItems()
        {
            LastSelectedTicketTag = null;
            LastSelectedOrderTag = null;

            foreach (var item in Orders)
                item.NotSelected();

            RefreshVisuals();

            this.PublishEvent(EventTopicNames.SelectedOrdersChanged);
        }

        public void RefreshVisuals()
        {
            RaisePropertyChanged(() => TicketTotalLabel);
            RaisePropertyChanged(() => TicketRemainingLabel);
            RaisePropertyChanged(() => TicketDiscountLabel);
            RaisePropertyChanged(() => TicketPlainTotalLabel);
            RaisePropertyChanged(() => TicketServiceLabel);
            RaisePropertyChanged(() => TicketRoundingLabel);
            RaisePropertyChanged(() => TicketTaxLabel);
            RaisePropertyChanged(() => IsTicketRemainingVisible);
            RaisePropertyChanged(() => IsTicketPaymentVisible);
            RaisePropertyChanged(() => IsTicketDiscountVisible);
            RaisePropertyChanged(() => IsTicketTotalVisible);
            RaisePropertyChanged(() => IsTicketTaxTotalVisible);
            RaisePropertyChanged(() => IsTicketRoundingVisible);
            RaisePropertyChanged(() => IsTicketServiceVisible);
            RaisePropertyChanged(() => IsTagged);
        }

        public bool CanCancelSelectedItems()
        {
            return Model.CanCancelSelectedOrders(SelectedOrders.Select(x => x.Model));
        }

        public bool CanCloseTicket()
        {
            return !_forcePayment || Model.GetRemainingAmount() <= 0 || !string.IsNullOrEmpty(Location) || !string.IsNullOrEmpty(AccountName) || IsTagged || Orders.Count == 0;
        }

        public bool IsTicketTotalVisible
        {
            get { return TicketPaymentValue > 0 && TicketTotalValue > 0; }
        }

        public bool IsTicketPaymentVisible
        {
            get { return TicketPaymentValue > 0; }
        }

        public bool IsTicketRemainingVisible
        {
            get { return TicketRemainingValue > 0; }
        }

        public bool IsTicketTaxTotalVisible
        {
            get { return TicketTaxValue > 0; }
        }

        public bool IsPlainTotalVisible
        {
            get { return IsTicketDiscountVisible || IsTicketTaxTotalVisible || IsTicketRoundingVisible || IsTicketServiceVisible; }
        }

        public bool IsTicketDiscountVisible
        {
            get { return TicketDiscountAmount != 0; }
        }

        public bool IsTicketRoundingVisible
        {
            get { return TicketRoundingAmount != 0; }
        }

        public bool IsTicketServiceVisible
        {
            get { return TicketServiceValue > 0; }
        }

        public string Location
        {
            get { return Model.LocationName; }
            set { Model.LocationName = value; }
        }

        public int AccountId
        {
            get { return Model.AccountId; }
            set { Model.AccountId = value; }
        }

        public string AccountName
        {
            get { return Model.AccountName; }
            set { Model.AccountName = value; }
        }

        public bool IsLocked { get { return Model.Locked; } set { Model.Locked = value; } }
        public bool IsTagged { get { return Model.IsTagged; } }

        public void UpdatePaidItems(IEnumerable<PaidItem> paidItems)
        {
            Model.PaidItems.Clear();
            foreach (var paidItem in paidItems)
            {
                Model.PaidItems.Add(paidItem);
            }
        }

        public void FixSelectedItems()
        {
            var selectedItems = SelectedOrders.Where(x => x.SelectedQuantity > 0 && x.SelectedQuantity < x.Quantity).ToList();
            var newItems = Model.ExtractSelectedOrders(selectedItems.Select(x => x.Model));
            foreach (var newItem in newItems)
            {
                _ticketService.AddItemToSelectedTicket(newItem);
                _orders.Add(new OrderViewModel(newItem, _ticketTemplate, _menuService) { Selected = true });
            }
            selectedItems.ForEach(x => x.NotSelected());
        }

        public string Title
        {
            get
            {
                if (Model == null) return "";

                string selectedTicketTitle;

                if (!string.IsNullOrEmpty(Location) && Model.Id == 0)
                    selectedTicketTitle = string.Format(Resources.Location_f, Location);
                else if (!string.IsNullOrEmpty(AccountName) && Model.Id == 0)
                    selectedTicketTitle = string.Format(Resources.Account_f, AccountName);
                else if (string.IsNullOrEmpty(AccountName)) selectedTicketTitle = string.IsNullOrEmpty(Location)
                     ? string.Format("# {0}", Model.TicketNumber)
                     : string.Format(Resources.TicketNumberAndLocation_f, Model.TicketNumber, Location);
                else if (string.IsNullOrEmpty(Location)) selectedTicketTitle = string.IsNullOrEmpty(AccountName)
                     ? string.Format("# {0}", Model.TicketNumber)
                     : string.Format(Resources.TicketNumberAndAccount_f, Model.TicketNumber, AccountName);
                else selectedTicketTitle = string.Format(Resources.AccountNameAndLocationName_f, Model.TicketNumber, AccountName, Location);

                return selectedTicketTitle;
            }
        }

        public string CustomPrintData { get { return Model.PrintJobData; } set { Model.PrintJobData = value; } }

        public TicketTagGroup LastSelectedTicketTag { get; set; }
        public OrderTagGroup LastSelectedOrderTag { get; set; }

        public void MergeLines()
        {
            Model.MergeOrdersAndUpdateOrderNumbers(0);
            _orders.Clear();
            _orders.AddRange(Model.Orders.Select(x => new OrderViewModel(x, _ticketTemplate, _menuService)));
        }

        public bool CanMoveSelectedOrders()
        {
            if (IsLocked) return false;
            if (!Model.CanRemoveSelectedOrders(SelectedOrders.Select(x => x.Model))) return false;
            if (SelectedOrders.Where(x => x.Model.Id == 0).Count() > 0) return false;
            if (SelectedOrders.Where(x => x.IsLocked).Count() == 0
                && _userService.IsUserPermittedFor(PermissionNames.MoveUnlockedOrders))
                return true;
            return _userService.IsUserPermittedFor(PermissionNames.MoveOrders);
        }

        public bool CanChangeLocation()
        {
            if (IsLocked || Orders.Count == 0 || (Payments.Count > 0 && !string.IsNullOrEmpty(Location)) || !Model.CanSubmit) return false;
            return string.IsNullOrEmpty(Location) || _userService.IsUserPermittedFor(PermissionNames.ChangeLocation);
        }

        public string GetPrintError()
        {
            if (Orders.Count(x => x.TotalPrice == 0 && x.Model.CalculatePrice) > 0)
                return Resources.CantCompleteOperationWhenThereIsZeroPricedProduct;
            if (!IsPaid && Orders.Count > 0)
            {
                var tg = _ticketTemplate.TicketTagGroups.FirstOrDefault(x => x.ForceValue && !IsTaggedWith(x.Name));
                if (tg != null) return string.Format(Resources.TagCantBeEmpty_f, tg.Name);
            }
            return "";
        }

        public bool IsTaggedWith(string tagGroup)
        {
            return !string.IsNullOrEmpty(Model.GetTagValue(tagGroup));
        }
    }
}
