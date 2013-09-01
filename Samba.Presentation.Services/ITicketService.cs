using System;
using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Persistance;
using Samba.Persistance.Common;

namespace Samba.Presentation.Services
{
    public interface ITicketService
    {
        Ticket OpenTicket(int ticketId);
        TicketCommitResult CloseTicket(Ticket ticket);
        TicketCommitResult MoveOrders(Ticket ticket, Order[] selectedOrders, int targetTicketId);
        TicketCommitResult MergeTickets(IEnumerable<int> ticketIds);
        Order AddOrder(Ticket ticket, int menuItemId, decimal quantity, string portionName, string orderState);
        void AddPayment(Ticket ticket, PaymentType paymentType, Account account, decimal amount, decimal tenderedAmount);
        Account GetAccountForPayment(Ticket ticket, PaymentType paymentType);
        void AddChangePayment(Ticket ticket, ChangePaymentType paymentType, Account account, decimal amount);
        void PayTicket(Ticket ticket, PaymentType template);
        void UpdateTicketNumber(Ticket ticket, Numerator numerator);
        void UpdateEntity(Ticket ticket, Entity entity);
        void UpdateEntity(Ticket ticket, Entity entity, int accountTypeId, int accountId, string entityCustomData);
        void RecalculateTicket(Ticket ticket);
        void UpdateTag(Ticket ticket, TicketTagGroup tagGroup, TicketTag ticketTag);
        void UpdateTag(Ticket ticket, string tagName, string tagValue);
        void TagOrders(Ticket ticket, IEnumerable<Order> selectedOrders, OrderTagGroup orderTagGroup, OrderTag orderTag, string tagNote);
        void UntagOrders(Ticket ticket, IEnumerable<Order> selectedOrders, OrderTagGroup orderTagGroup, OrderTag orderTag);
        bool CanDeselectOrders(IEnumerable<Order> selectedOrders);
        bool CanDeselectOrder(Order order);
        OrderTagGroup GetMandantoryOrderTagGroup(Order order);
        bool CanCloseTicket(Ticket ticket);
        bool CanSettleTicket(Ticket ticket);
        void RefreshAccountTransactions(Ticket ticket);
        void UpdateOrderStates(Ticket ticket, IList<Order> orders, string stateName, string currentState, int groupOrder, string state, int stateOrder, string stateValue);
        void UpdateTicketState(Ticket ticket, string stateName, string currentState, string state, string stateValue, string quantityDef = "");
        void ChangeOrdersAccountTransactionTypeId(Ticket ticket, IEnumerable<Order> selectedOrders, int accountTransactionTypeId);
        void AddAccountTransaction(Ticket ticket, Account sourceAccount, Account targetAccount, decimal amount, decimal exchangeRate);
        bool CanMakeAccountTransaction(TicketEntity ticketEntity, AccountTransactionType accountTransactionType, decimal targetBalance);
        void UpdateOrderPrice(Order order, string portionName, string priceTag);
        void CancelSelectedOrders(Ticket ticket);
    }
}
