using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Services
{
    public class TicketTagData
    {
        public string TagName { get; set; }

        private string _tagValue;
        public string TagValue
        {
            get { return _tagValue ?? string.Empty; }
            set { _tagValue = value; }
        }

        public int Action { get; set; }
        public decimal NumericValue { get; set; }
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
        TicketCommitResult MoveOrders(Ticket ticket, IEnumerable<Order> selectedOrders, int targetTicketId);
        void ChangeTicketLocation(Ticket ticket, int locationId);
        TicketCommitResult CloseTicket(Ticket ticket);
        void AddPayment(Ticket ticket, decimal tenderedAmount, DateTime date, PaymentType paymentType);
        void PaySelectedTicket(Ticket ticket, PaymentType paymentType);
        void UpdateTicketNumber(Ticket ticket, Numerator numerator);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(MenuItem menuItem);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<MenuItem> menuItems);
        void UpdateAccount(Ticket ticket, Account account);
        void RecalculateTicket(Ticket ticket);
        void RegenerateTaxRates(Ticket ticket);
        void UpdateTag(Ticket ticket, TicketTagGroup tagGroup, TicketTag ticketTag);
        void ResetLocationData(Ticket ticket);
        void AddItemToSelectedTicket(Order newItem);
        IEnumerable<string> GetTicketTagGroupNames();
        int GetOpenTicketCount();
    }
}
