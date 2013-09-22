using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Persistance.Common;

namespace Samba.Services.Implementations.TicketModule
{
    public class TicketExplorerFilter : ITicketExplorerFilter
    {
        private readonly ICacheService _cacheService;

        public TicketExplorerFilter(ICacheService cacheService)
        {
            _cacheService = cacheService;
            FilterValues = new List<string>();
        }

        public bool IsTextBoxEnabled { get { return true; } }

        private IEnumerable<string> _filterTypes;
        public IEnumerable<string> FilterTypes { get { return _filterTypes ?? (_filterTypes = CreateFilterTypes()); } }

        private IEnumerable<string> CreateFilterTypes()
        {
            var result = new List<string> { Resources.OnlyOpenTickets, Resources.AllTickets, Resources.TicketNumber, Resources.TicketNote };
            _cacheService.GetEntityTypes().Select(x => x.EntityName).ToList().ForEach(result.Add);
            return result;
        }

        public string FilterType { get; set; }
        public string FilterValue { get; set; }
        public List<string> FilterValues { get; set; }

        public Expression<Func<Ticket, bool>> GetExpression()
        {
            var fv = (FilterValue ?? "").ToLower();
            Expression<Func<Ticket, bool>> result;
            if (FilterType == Resources.AllTickets)
            {
                if (!string.IsNullOrEmpty(FilterValue))
                {
                    result = x => x.Orders.Any(y => y.MenuItemName.ToLower().Contains(fv));
                }
                else result = x => x.Id > 0;
            }
            else if (FilterType == Resources.OnlyOpenTickets)
                if (!string.IsNullOrEmpty(FilterValue))
                {
                    result = x => !x.IsClosed && x.Orders.Any(y => y.MenuItemName.ToLower().Contains(fv));
                }
                else result = x => !x.IsClosed;
            else if (FilterType == Resources.TicketNumber)
            {
                result = x => x.TicketNumber != null && x.TicketNumber.ToLower() == fv;
            }
            else if (FilterType == Resources.TicketNote)
            {
                result = x => x.Note != null && x.Note.Contains(FilterValue);
            }
            else
            {
                var tid = 0;
                var rt = _cacheService.GetEntityTypes().SingleOrDefault(x => x.EntityName == FilterType);
                if (rt != null) tid = rt.Id;
                result = x => x.TicketEntities.Any(y => y.EntityTypeId == tid && y.EntityName.ToLower().Contains(fv));
            }
            return result;
        }
    }
}
