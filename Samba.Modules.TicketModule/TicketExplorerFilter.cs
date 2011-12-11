using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Modules.TicketModule
{
    public enum FilterType
    {
        OpenTickets,
        AllTickets,
        Account,
        Location
    }

    public class TicketExplorerFilter : ObservableObject
    {
        public TicketExplorerFilter()
        {
            FilterValues = new List<string>();
        }

        private readonly string[] _filterTypes = { Resources.OnlyOpenTickets, Resources.AllTickets, Resources.Account, Resources.Location};
        public int FilterTypeIndex
        {
            get { return (int)FilterType; }
            set
            {
                FilterType = (FilterType)value;
                FilterValue = "";
                RaisePropertyChanged(() => IsTextBoxEnabled);
            }
        }

        public bool IsTextBoxEnabled { get { return FilterType != FilterType.OpenTickets; } }
        public string[] FilterTypes { get { return _filterTypes; } }

        public FilterType FilterType { get; set; }

        private string _filterValue;
        public string FilterValue
        {
            get { return _filterValue; }
            set
            {
                _filterValue = value;
                RaisePropertyChanged(() => FilterValue);
            }
        }

        public List<string> FilterValues { get; set; }

        public Expression<Func<Ticket, bool>> GetExpression()
        {
            Expression<Func<Ticket, bool>> result = null;

            if (FilterType == FilterType.OpenTickets)
                result = x => !x.IsPaid;

            if (FilterType == FilterType.Location)
            {
                if (FilterValue == "*")
                    result = x => !string.IsNullOrEmpty(x.LocationName);
                else if (!string.IsNullOrEmpty(FilterValue))
                    result = x => x.LocationName.ToLower() == FilterValue.ToLower();
                else result = x => string.IsNullOrEmpty(x.LocationName);
            }

            if (FilterType == FilterType.Account)
            {
                if (FilterValue == "*")
                    result = x => !string.IsNullOrEmpty(x.AccountName);
                else if (!string.IsNullOrEmpty(FilterValue))
                    result = x => x.AccountName.ToLower().Contains(FilterValue.ToLower());
                else
                    result = x => string.IsNullOrEmpty(x.AccountName);
            }

            return result;
        }
    }
}
