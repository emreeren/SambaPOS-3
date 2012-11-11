using System;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;

namespace Samba.Services.Implementations
{
    [Export(typeof(ITicketService))]
    class TicketService : ITicketService
    {
        [ImportingConstructor]
        public TicketService()
        {
            ValidatorRegistry.RegisterConcurrencyValidator(new TicketConcurrencyValidator());
        }

        public void AddPayment(Ticket ticket, PaymentType paymentType, Account account, decimal tenderedAmount, decimal exchangeRate, int userId)
        {
            if (account == null) throw new ArgumentNullException("account");
            ticket.AddPayment(paymentType, account, tenderedAmount, exchangeRate, userId);
        }
    }

    public class TicketConcurrencyValidator : ConcurrencyValidator<Ticket>
    {
        public override ConcurrencyCheckResult GetErrorMessage(Ticket current, Ticket loaded)
        {
            if (current.Id > 0)
            {
                if (current.AccountName != loaded.AccountName)
                {
                    return ConcurrencyCheckResult.Break(string.Format(Resources.TicketMovedRetryLastOperation_f, loaded.AccountName));
                }

                if (current.TicketResources.Count != loaded.TicketResources.Count || !current.TicketResources.All(x => loaded.TicketResources.Any(y => x.ResourceId == y.ResourceId)))
                {
                    var resource = current.TicketResources.FirstOrDefault(x => loaded.TicketResources.All(y => y.ResourceId != x.ResourceId))
                        ?? loaded.TicketResources.First(x => current.TicketResources.All(y => y.ResourceId != x.ResourceId));
                    return ConcurrencyCheckResult.Break(string.Format(Resources.TicketMovedRetryLastOperation_f, resource.ResourceName));
                }

                if (current.IsClosed != loaded.IsClosed)
                {
                    if (loaded.IsClosed)
                    {
                        return ConcurrencyCheckResult.Break(Resources.TicketPaidChangesNotSaved);
                    }
                    if (current.IsClosed)
                    {
                        return ConcurrencyCheckResult.Break(Resources.TicketChangedRetryLastOperation);
                    }
                }
                else if (current.LastPaymentDate != loaded.LastPaymentDate)
                {
                    var currentPaymentIds = current.Payments.Select(x => x.Id).Distinct();
                    var unknownPayments = loaded.Payments.FirstOrDefault(x => !currentPaymentIds.Contains(x.Id));
                    if (unknownPayments != null)
                    {
                        return ConcurrencyCheckResult.Break(Resources.TicketPaidLastChangesNotSaved);
                    }
                }

                if (current.RemainingAmount == 0 && loaded.GetSum() != current.GetSum())
                {
                    return ConcurrencyCheckResult.Break(Resources.TicketChangedRetryLastOperation);
                }
            }

            return ConcurrencyCheckResult.Continue();
        }
    }
}
