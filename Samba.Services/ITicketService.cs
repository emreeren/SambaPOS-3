using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public class TicketCommitResult
    {
        public int TicketId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface ITicketService : IService
    {
        void OpenTicket(int ticketId);
        void OpenTicketByLocationName(string locationName);
        void OpenTicketByTicketNumber(string ticketNumber);
        TicketCommitResult MoveOrders(IEnumerable<Order> selectedOrders, int targetTicketId);
        void ChangeTicketLocation(Ticket ticket, int locationId);
        Ticket CurrentTicket { get; }
        TicketCommitResult CloseTicket();
        void AddPayment(decimal tenderedAmount, DateTime date, PaymentType paymentType);
        void PaySelectedTicket(PaymentType paymentType);
        void UpdateTicketNumber(Ticket ticket, Numerator numerator);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(MenuItem menuItem);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<MenuItem> menuItems);
        void UpdateAccount(Ticket ticket, Account account);
        void RecalculateTicket(Ticket ticket);
        void RegenerateTaxRates(Ticket ticket);
        void UpdateTag(Ticket ticket, TicketTagGroup tagGroup, TicketTag ticketTag);
        void ResetLocationData(Ticket ticket);
        void AddItemToSelectedTicket(Order newItem);
    }
}
