using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Persistance;

namespace Samba.Presentation.Services
{
    public interface ITicketService
    {
        Ticket OpenTicket(int ticketId);
        TicketCommitResult CloseTicket(Ticket ticket);
        TicketCommitResult MoveOrders(Ticket ticket, Order[] selectedOrders, int targetTicketId);
        TicketCommitResult MergeTickets(IEnumerable<int> ticketIds);
        Order AddOrder(Ticket ticket, int menuItemId, decimal quantity, string portionName, OrderTagTemplate template);
        void AddPayment(Ticket ticket, PaymentType paymentType, Account account, decimal tenderedAmount);
        void AddChangePayment(Ticket ticket, ChangePaymentType paymentType, Account account, decimal amount);
        void PayTicket(Ticket ticket, PaymentType template);
        void UpdateTicketNumber(Ticket ticket, Numerator numerator);
        void UpdateResource(Ticket ticket, Resource resource);
        void UpdateResource(Ticket ticket, int resourceTypeId, int resourceId, string resourceName, int accountId, string resourceCustomData);
        void RecalculateTicket(Ticket ticket);
        void RegenerateTaxRates(Ticket ticket);
        void UpdateTag(Ticket ticket, TicketTagGroup tagGroup, TicketTag ticketTag);
        void UpdateState(Ticket ticket, string group, string state, string stateValue, int quantity = 0);
        IEnumerable<Order> ExtractSelectedOrders(Ticket model, IEnumerable<Order> selectedOrders);
        int GetOpenTicketCount();
        IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction);
        IEnumerable<OpenTicketData> GetOpenTickets(int resourceId);
        IEnumerable<int> GetOpenTicketIds(int resourceId);
        IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters);
        IList<ITicketExplorerFilter> CreateTicketExplorerFilters();
        void UpdateAccountOfOpenTickets(Resource resource);
        IEnumerable<Order> GetOrders(int id);
        void TagOrders(Ticket ticket, IEnumerable<Order> selectedOrders, OrderTagGroup orderTagGroup, OrderTag orderTag, string tagNote);
        void UntagOrders(Ticket ticket, IEnumerable<Order> selectedOrders, OrderTagGroup orderTagGroup, OrderTag orderTag);
        bool CanDeselectOrders(IEnumerable<Order> selectedOrders);
        bool CanDeselectOrder(Order order);
        OrderTagGroup GetMandantoryOrderTagGroup(Order order);
        bool CanCloseTicket(Ticket ticket);
        void RefreshAccountTransactions(Ticket ticket);
        void UpdateOrderStates(Ticket ticket, IEnumerable<Order> selectedOrders, OrderStateGroup orderStateGroup, OrderState orderState);
        void ChangeOrdersAccountTransactionTypeId(Ticket ticket, IEnumerable<Order> selectedOrders, int accountTransactionTypeId);
        void AddAccountTransaction(Ticket ticket, Account sourceAccount, Account targetAccount, decimal amount, decimal exchangeRate);
    }
}
