using Samba.Domain.Models.Tickets;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class CalculationSelectorMapViewModel : AbstractMapViewModel
    {
        public CalculationSelectorMap Model { get; set; }

        public CalculationSelectorMapViewModel(CalculationSelectorMap model, IUserService userService, IDepartmentService departmentService,ISettingService settingService)
            : base(model, userService, departmentService,settingService)
        {
            Model = model;
        }
    }
}
