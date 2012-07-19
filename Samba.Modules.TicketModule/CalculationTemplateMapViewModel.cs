using Samba.Domain.Models.Tickets;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class CalculationTemplateMapViewModel : AbstractMapViewModel
    {
        public CalculationTemplateMap Model { get; set; }

        public CalculationTemplateMapViewModel(CalculationTemplateMap model, IUserService userService, IDepartmentService departmentService,ISettingService settingService)
            : base(model, userService, departmentService,settingService)
        {
            Model = model;
        }
    }
}
