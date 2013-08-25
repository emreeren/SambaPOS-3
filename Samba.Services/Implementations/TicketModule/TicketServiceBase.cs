using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Persistance.Common;
using Samba.Persistance.Data;

namespace Samba.Services.Implementations.TicketModule
{
    [Export(typeof(ITicketServiceBase))]
    class TicketServiceBase : ITicketServiceBase
    {
        private readonly ITicketDao _ticketDao;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public TicketServiceBase(ITicketDao ticketDao, ICacheService cacheService)
        {
            _ticketDao = ticketDao;
            _cacheService = cacheService;
        }

        public IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters)
        {
            return _ticketDao.GetFilteredTickets(startDate, endDate, filters);
        }

        public IList<ITicketExplorerFilter> CreateTicketExplorerFilters()
        {
            var item = new TicketExplorerFilter(_cacheService) { FilterType = Resources.OnlyOpenTickets };
            return new List<ITicketExplorerFilter> { item };
        }

        public IEnumerable<Order> GetOrders(int ticketId)
        {
            return _ticketDao.GetOrders(ticketId);
        }

        public IEnumerable<int> GetOpenTicketIds(int entityId)
        {
            return _ticketDao.GetOpenTicketIds(entityId);
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(int entityId)
        {
            return GetOpenTickets(x => !x.IsClosed && x.TicketEntities.Any(y => y.EntityId == entityId));
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction)
        {
            return _ticketDao.GetOpenTickets(prediction);
        }

        public int GetOpenTicketCount()
        {
            return _ticketDao.GetOpenTicketCount();
        }

        public void UpdateAccountOfOpenTickets(Entity entity)
        {
            var openTicketDataList = GetOpenTickets(entity.Id).Select(x => x.Id);
            using (var w = WorkspaceFactory.Create())
            {
                var tickets = w.All<Ticket>(x => openTicketDataList.Contains(x.Id), x => x.TicketEntities);
                foreach (var ticket in tickets)
                {
                    ticket.TicketEntities.Where(x => x.EntityId == entity.Id).ToList().ForEach(x =>
                    {
                        var entityType = _cacheService.GetEntityTypeById(x.EntityTypeId);
                        x.AccountTypeId = entityType.AccountTypeId;
                        x.AccountId = entity.AccountId;
                    });
                }
                w.CommitChanges();
            }
        }

        public IEnumerable<Ticket> GetTicketsByState(string state)
        {
            var sv = string.Format("\"S\":\"{0}\"", state);
            var result = Dao.Query<Ticket>(x => x.TicketStates.Contains(sv),
                                           x => x.Orders.Select(y => y.ProductTimerValue),
                                           x => x.TicketEntities,
                                           x => x.Calculations,
                                           x => x.Payments,
                                           x => x.ChangePayments)
                            .ToList();
            result.ForEach(x => x.TransactionDocument = new AccountTransactionDocument());
            return result;
        }
    }
}
