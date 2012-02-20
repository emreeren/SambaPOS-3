using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class PeriodicConsumption : Entity
    {
        public int WorkPeriodId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        private readonly IList<PeriodicConsumptionItem> _periodicConsumptionItems;
        public virtual IList<PeriodicConsumptionItem> PeriodicConsumptionItems
        {
            get { return _periodicConsumptionItems; }
        }

        private readonly IList<CostItem> _costItems;
        public virtual IList<CostItem> CostItems
        {
            get { return _costItems; }
        }

        public PeriodicConsumption()
        {
            _periodicConsumptionItems = new List<PeriodicConsumptionItem>();
            _costItems = new List<CostItem>();
        }
    }
}
