using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    public class TicketViewModel : ObservableObject
    {
        private readonly Ticket _model;
        private readonly TicketTemplate _ticketTemplate;
        private readonly bool _forcePayment;
        private readonly ITicketService _ticketService;
        private readonly IAutomationService _automationService;
        private readonly IApplicationState _applicationState;

        public TicketViewModel(Ticket model, TicketTemplate ticketTemplate, bool forcePayment,
            ITicketService ticketService, IAutomationService automationService,
            IApplicationState applicationState)
        {
            _ticketService = ticketService;
            _forcePayment = forcePayment;
            _model = model;
            _ticketTemplate = ticketTemplate;
            _automationService = automationService;
            _applicationState = applicationState;

            _orders = new ObservableCollection<OrderViewModel>(model.Orders.Select(x => new OrderViewModel(x, ticketTemplate, _automationService)).OrderBy(x => x.Model.CreatedDateTime));
            _itemsViewSource = new CollectionViewSource { Source = _orders };
            _itemsViewSource.GroupDescriptions.Add(new PropertyGroupDescription("GroupObject"));

            SelectAllItemsCommand = new CaptionCommand<string>("", OnSelectAllItemsExecute);

            PrintJobButtons = _applicationState.CurrentTerminal.PrintJobs
                .Where(x => (!string.IsNullOrEmpty(x.ButtonHeader))
                    && (x.PrinterMaps.Count(y => y.DepartmentId == 0 || y.DepartmentId == model.DepartmentId) > 0))
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
            var so = new SelectedOrdersData { SelectedOrders = SelectedOrders.Select(x => x.Model), Ticket = Model };
            so.PublishEvent(EventTopicNames.SelectedOrdersChanged);
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
        //public bool IsLastPaymentDateVisible { get { return Model.Payments.Count > 0; } }
        public bool IsLastPaymentDateVisible { get { return false; } }
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
            foreach (var item in Orders)
                item.NotSelected();

            RefreshVisuals();
            var so = new SelectedOrdersData { SelectedOrders = SelectedOrders.Select(x => x.Model), Ticket = Model };
            so.PublishEvent(EventTopicNames.SelectedOrdersChanged);
        }

        public void RefreshVisuals()
        {
            RaisePropertyChanged(() => IsTagged);
        }

        public bool CanCancelSelectedItems()
        {
            return Model.CanCancelSelectedOrders(SelectedOrders.Select(x => x.Model));
        }

        public bool CanCloseTicket()
        {
            return !_forcePayment || Model.GetRemainingAmount() <= 0 || !string.IsNullOrEmpty(AccountName) || IsTagged || Orders.Count == 0;
        }

        public int AccountId { get { return Model.AccountId; } }
        public string AccountName { get { return Model.AccountName; } }
        public int AccountTemplateId { get { return Model.AccountTemplateId; } }

        public bool IsLocked { get { return Model.Locked; } set { Model.Locked = value; } }
        public bool IsTagged { get { return Model.IsTagged; } }

        public void FixSelectedItems()
        {
            var selectedItems = SelectedOrders.Where(x => x.SelectedQuantity > 0 && x.SelectedQuantity < x.Quantity).ToList();
            if (selectedItems.Count > 0)
            {
                var newItems = _ticketService.ExtractSelectedOrders(Model, selectedItems.Select(x => x.Model));
                foreach (var newItem in newItems)
                {
                    _orders.Add(new OrderViewModel(newItem, _ticketTemplate, _automationService) { Selected = true });
                }
                selectedItems.ForEach(x => x.NotSelected());
            }
        }

        public string CustomPrintData { get { return Model.PrintJobData; } set { Model.PrintJobData = value; } }



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
