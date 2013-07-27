using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Modules.TicketModule
{
    public class MenuAssignmentViewModel
    {
        private readonly IEnumerable<ScreenMenu> _menus;
        protected MenuAssignment Model { get; set; }

        public MenuAssignmentViewModel(MenuAssignment model, IEnumerable<ScreenMenu> menus)
        {
            _menus = menus;
            Model = model;
        }

        public string TerminalName { get { return Model.TerminalName; } set { Model.TerminalName = value; } }

        public IEnumerable<ScreenMenu> ScreenMenus { get { return _menus; } }

        public IEnumerable<string> MenuNames { get { return ScreenMenus.Select(x => x.Name); } }

        public string MenuName
        {
            get { return ScreenMenus.Where(x => x.Id == Model.MenuId).Select(x => x.Name).FirstOrDefault(); }
            set { Model.MenuId = ScreenMenus.Where(x => x.Name == value).Select(x => x.Id).FirstOrDefault(); }
        }
    }
}