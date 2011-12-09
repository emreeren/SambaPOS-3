using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface ITicketService : IService
    {
        void OpenTicket(int ticketId);
        TicketCommitResult MoveOrders(IEnumerable<Order> selectedOrders, int targetTicketId);
        void UpdateAccount(Account account);
        void UpdateLocation(int locationId);
        Ticket CurrentTicket { get; }
        TicketCommitResult CloseTicket();
        void AddPayment(decimal tenderedAmount, DateTime date, PaymentType paymentType);
        void PaySelectedTicket(PaymentType paymentType);
        void UpdateTicketNumber(Ticket ticket, Numerator numerator);
    }
}
