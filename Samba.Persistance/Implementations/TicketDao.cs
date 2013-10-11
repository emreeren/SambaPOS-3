using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Omu.ValueInjecter;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Injection;
using Samba.Infrastructure.Data.Validation;
using Samba.Localization.Properties;
using Samba.Persistance.Common;
using Samba.Persistance.Data;
using Samba.Persistance.Specification;

namespace Samba.Persistance.Implementations
{
    [Export(typeof(ITicketDao))]
    class TicketDao : ITicketDao
    {
        [ImportingConstructor]
        public TicketDao()
        {
            ValidatorRegistry.RegisterDeleteValidator<TicketType>(x => Dao.Exists<EntityScreen>(y => y.TicketTypeId == x.Id), Resources.TicketType, Resources.EntityScreen);
            ValidatorRegistry.RegisterConcurrencyValidator(new TicketConcurrencyValidator());
        }

        public void Save(Ticket ticket)
        {
            Dao.Save(ticket);
        }

        public TicketCommitResult CheckConcurrency(Ticket ticket)
        {
            return new TicketCommitResult { ErrorMessage = Dao.CheckConcurrency(ticket) };
        }

        public Ticket OpenTicket(int ticketId)
        {
            if (ticketId == 0) throw new ArgumentException("Ticket Id should be more than 0");
            return Dao.Load<Ticket>(ticketId,
                             x => x.Orders.Select(y => y.ProductTimerValue),
                             x => x.TicketEntities,
                             x => x.Calculations,
                             x => x.Payments,
                             x => x.PaidItems,
                             x => x.ChangePayments);
        }

        public int GetOpenTicketCount()
        {
            return Dao.Count<Ticket>(x => !x.IsClosed);
        }

        public IEnumerable<int> GetOpenTicketIds(int entityId)
        {
            return Dao.Select<Ticket, int>(x => x.Id, x => !x.IsClosed && x.TicketEntities.Any(y => y.EntityId == entityId));
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction)
        {
            return Dao.Select(x => new OpenTicketData
            {
                Id = x.Id,
                LastOrderDate = x.LastOrderDate,
                TicketNumber = x.TicketNumber,
                RemainingAmount = x.RemainingAmount,
                Date = x.Date,
                TicketEntities = x.TicketEntities,
                TicketTags = x.TicketTags
            }, prediction, x => x.TicketEntities);
        }

        public void SaveFreeTicketTag(int ticketTagGroupId, string freeTag)
        {
            if (string.IsNullOrEmpty(freeTag)) return;

            using (var workspace = WorkspaceFactory.Create())
            {
                var tt = workspace.Single<TicketTagGroup>(x => x.Id == ticketTagGroupId);
                Debug.Assert(tt != null);
                var tag = tt.TicketTags.FirstOrDefault(x => x.Name.ToLower() == freeTag.ToLower());
                if (tag != null) return;
                tag = new TicketTag { Name = freeTag };
                tt.TicketTags.Add(tag);
                workspace.Add(tag);
                workspace.CommitChanges();
            }
        }

        public IEnumerable<Ticket> GetAllTickets()
        {
            return Dao.Query<Ticket>(x => x.Orders.Select(y => y.ProductTimerValue),
                             x => x.TicketEntities,
                             x => x.Calculations,
                             x => x.Payments,
                             x => x.ChangePayments);
        }

        Ticket ITicketDao.GetTicketById(int id)
        {
            return Dao.Single<Ticket>(x => x.Id == id,
                             x => x.Orders.Select(y => y.ProductTimerValue),
                             x => x.TicketEntities,
                             x => x.Calculations,
                             x => x.Payments,
                             x => x.ChangePayments);
        }

        public IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters)
        {
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);
            Expression<Func<Ticket, bool>> qFilter = x => x.Date >= startDate && x.Date < endDate;
            qFilter = filters.Aggregate(qFilter, (current, filter) => current.And(filter.GetExpression()));
            return Dao.Query(qFilter, x => x.TicketEntities);
        }

        public IEnumerable<Order> GetOrders(int ticketId)
        {
            return Dao.Query<Order>(x => x.TicketId == ticketId);
        }

        public void SaveFreeOrderTag(int orderTagGroupId, OrderTag orderTag)
        {
            using (var v = WorkspaceFactory.Create())
            {
                var og = v.Single<OrderTagGroup>(x => x.Id == orderTagGroupId);
                if (og != null)
                {
                    var lvTagName = orderTag.Name.ToLower();
                    var t = v.Single<OrderTag>(x => x.Name.ToLower() == lvTagName);
                    if (t == null)
                    {
                        var ot = new OrderTag();
                        ot.InjectFrom<CloneInjection>(orderTag);
                        og.OrderTags.Add(ot);
                        v.CommitChanges();
                    }
                }
            }
        }
    }

    public class TicketConcurrencyValidator : ConcurrencyValidator<Ticket>
    {
        public override ConcurrencyCheckResult GetErrorMessage(Ticket current, Ticket loaded)
        {
            if (current.Id > 0)
            {
                if (current.TicketEntities.Count != loaded.TicketEntities.Count || !current.TicketEntities.All(x => loaded.TicketEntities.Any(y => x.EntityId == y.EntityId)))
                {
                    var entity = current.TicketEntities.FirstOrDefault(x => loaded.TicketEntities.All(y => y.EntityId != x.EntityId))
                        ?? loaded.TicketEntities.First(x => current.TicketEntities.All(y => y.EntityId != x.EntityId));
                    return ConcurrencyCheckResult.Break(string.Format(Resources.TicketMovedRetryLastOperation_f, entity.EntityName));
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
