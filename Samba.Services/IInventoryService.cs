using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IInventoryService : IService
    {
        PeriodicConsumption GetPreviousPeriodicConsumption(IWorkspace workspace);
        PeriodicConsumption GetCurrentPeriodicConsumption(IWorkspace workspace);
        void CalculateCost(PeriodicConsumption pc, WorkPeriod workPeriod);
        IEnumerable<string> GetInventoryItemNames();
    }
}
