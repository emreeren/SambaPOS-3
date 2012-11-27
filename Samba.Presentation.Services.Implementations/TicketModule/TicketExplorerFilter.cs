using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.TicketModule
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
            var result = new List<string> { Resources.OnlyOpenTickets, Resources.AllTickets };
            _cacheService.GetResourceTypes().Select(x => x.EntityName).ToList().ForEach(result.Add);
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
            else
            {
                var tid = 0;
                var rt = _cacheService.GetResourceTypes().SingleOrDefault(x => x.EntityName == FilterType);
                if (rt != null) tid = rt.Id;
                result = x => x.TicketResources.Any(y => y.ResourceTypeId == tid && y.ResourceName.ToLower().Contains(fv));
            }
            return result;
        }
    }
}
