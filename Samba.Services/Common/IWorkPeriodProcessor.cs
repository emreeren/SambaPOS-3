using Samba.Domain.Models.Settings;

namespace Samba.Services.Common
{
    public interface IWorkPeriodProcessor
    {
        void ProcessWorkPeriodStart(WorkPeriod workPeriod);
        void ProcessWorkPeriodEnd(WorkPeriod workPeriod);
    }
}