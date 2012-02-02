using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Services
{
    public class OpenTicketData
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastOrderDate { get; set; }
        public string TicketNumber { get; set; }
        public string LocationName { get; set; }
        public string AccountName { get; set; }
        public decimal RemainingAmount { get; set; }
    }

    public class TicketExplorerRowData
    {
        public TicketExplorerRowData(Ticket model)
        {
            Model = model;
        }

        public Ticket Model { get; set; }
        public int Id { get { return Model.Id; } }
        public string TicketNumber { get { return Model.TicketNumber; } }
        public string Location { get { return Model.LocationName; } }
        public string Date { get { return Model.Date.ToShortDateString(); } }
        public string AccountName { get { return Model.SaleTransaction.TargetTransactionValue.AccountName; } }
        public string CreationTime { get { return Model.Date.ToShortTimeString(); } }
        public string LastPaymentTime { get { return Model.LastPaymentDate.ToShortTimeString(); } }
        public decimal Sum { get { return Model.TotalAmount; } }
        public bool IsPaid { get { return Model.IsPaid; } }
        public string TimeInfo { get { return CreationTime != LastPaymentTime || IsPaid ? CreationTime + " - " + LastPaymentTime : CreationTime; } }
    }

    public class TicketTagData
    {
        public string TagName { get; set; }

        private string _tagValue;
        public string TagValue
        {
            get { return _tagValue ?? string.Empty; }
            set { _tagValue = value; }
        }

        public TicketTagGroup TicketTagGroup { get; set; }
        public Ticket Ticket { get; set; }
    }

    public class TicketCommitResult
    {
        public int TicketId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface ITicketService : IService
    {
        Ticket OpenTicket(int ticketId);
        //todo move to state
        Ticket OpenTicketByLocationName(string locationName);
        Ticket OpenTicketByTicketNumber(string ticketNumber);
        TicketCommitResult CloseTicket(Ticket ticket);
        TicketCommitResult MoveOrders(Ticket ticket, IEnumerable<Order> selectedOrders, int targetTicketId);

        void ChangeTicketLocation(Ticket ticket, int locationId);
        void AddPayment(Ticket ticket, decimal tenderedAmount, DateTime date, PaymentType paymentType);
        void PaySelectedTicket(Ticket ticket, PaymentType paymentType);
        void UpdateTicketNumber(Ticket ticket, Numerator numerator);
        void UpdateAccount(Ticket ticket, Account account);
        void RecalculateTicket(Ticket ticket);
        void RegenerateTaxRates(Ticket ticket);
        void UpdateTag(Ticket ticket, TicketTagGroup tagGroup, TicketTag ticketTag);
        void ResetLocationData(Ticket ticket);
        IEnumerable<Order> ExtractSelectedOrders(Ticket model, IEnumerable<Order> selectedOrders);
        int GetOpenTicketCount();
        IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction);
        IList<TicketExplorerRowData> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters);
        IList<ITicketExplorerFilter> CreateTicketExplorerFilters();
    }
}
