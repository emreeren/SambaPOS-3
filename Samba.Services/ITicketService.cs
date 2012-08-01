using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Services.Common;

namespace Samba.Services
{
    public class OpenTicketData
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastOrderDate { get; set; }
        public string TicketNumber { get; set; }
        public decimal RemainingAmount { get; set; }
        public IEnumerable<TicketResource> TicketResources { get; set; }
        public string TicketTags { get; set; }

        private List<TicketTagValue> _ticketTagValues;
        public List<TicketTagValue> TicketTagValues
        {
            get { return _ticketTagValues ?? (_ticketTagValues = JsonHelper.Deserialize<List<TicketTagValue>>(TicketTags)); }
        }
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
        TicketCommitResult CloseTicket(Ticket ticket);
        TicketCommitResult MoveOrders(Ticket ticket, Order[] selectedOrders, int targetTicketId);
        TicketCommitResult MergeTickets(IEnumerable<int> ticketIds);
        Order AddOrder(Ticket ticket, int menuItemId, decimal quantity, string portionName, OrderTagTemplate template);
        void AddPayment(Ticket ticket, string paymentTemplateName, AccountTransactionTemplate transactionTemplate, Account account, decimal tenderedAmount);
        void PayTicket(Ticket ticket, PaymentTemplate template);
        void UpdateTicketNumber(Ticket ticket, Numerator numerator);
        void UpdateAccount(Ticket ticket, Account account);
        void UpdateResource(Ticket ticket, Resource resource);
        void UpdateResource(Ticket ticket, int resourceTemplateId, int resourceId, string resourceName, int accountId);
        void RecalculateTicket(Ticket ticket);
        void RegenerateTaxRates(Ticket ticket);
        void UpdateTag(Ticket ticket, TicketTagGroup tagGroup, TicketTag ticketTag);
        IEnumerable<Order> ExtractSelectedOrders(Ticket model, IEnumerable<Order> selectedOrders);
        int GetOpenTicketCount();
        IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction);
        IEnumerable<OpenTicketData> GetOpenTickets(int resourceId);
        IEnumerable<int> GetOpenTicketIds(int resourceId);
        IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters);
        IList<ITicketExplorerFilter> CreateTicketExplorerFilters();
        void UpdateAccountOfOpenTickets(Resource resource);
        IEnumerable<Order> GetOrders(int id);
    }
}
