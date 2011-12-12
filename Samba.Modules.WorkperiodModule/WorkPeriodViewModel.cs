using Samba.Domain.Models.Settings;

namespace Samba.Modules.WorkperiodModule
{
    public class WorkPeriodViewModel
    {
        public WorkPeriod Model { get; set; }

        public WorkPeriodViewModel(WorkPeriod model)
        {
            Model = model;
        }

        public string WorkPeriodLabel
        {
            get
            {
                if (Model.EndDate > Model.StartDate)
                    return
                        Model.StartDate.ToLongDateString() + " " + Model.StartDate.ToShortTimeString() + " - " +
                        Model.EndDate.ToLongDateString() + " " + Model.EndDate.ToShortTimeString();

                return Model.StartDate.ToLongDateString() + " " + Model.StartDate.ToShortTimeString();
            }
        }
    }
}
