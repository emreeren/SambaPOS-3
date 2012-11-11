using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface ITicketService
    {
        void AddPayment(Ticket ticket, PaymentType paymentType, Account account, decimal tenderedAmount,decimal exchangeRate,int userId);
    }
}
