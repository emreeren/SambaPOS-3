using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class TicketTagMapViewModel : AbstractMapViewModel
    {
        public TicketTagMapViewModel(TicketTagMap model, IUserService userService, IDepartmentService departmentService)
            : base(model, userService, departmentService)
        {
            Model = model;
        }

        public TicketTagMap Model { get; set; }
    }
}
