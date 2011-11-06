using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class PeriodicConsumption : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int WorkPeriodId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual IList<PeriodicConsumptionItem> PeriodicConsumptionItems { get; set; }
        public virtual IList<CostItem> CostItems { get; set; }

        public PeriodicConsumption()
        {
            PeriodicConsumptionItems = new List<PeriodicConsumptionItem>();
            CostItems = new List<CostItem>();
        }
    }
}
