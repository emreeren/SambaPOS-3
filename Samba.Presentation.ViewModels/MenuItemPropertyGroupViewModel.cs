using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;

namespace Samba.Presentation.ViewModels
{
    public class MenuItemPropertyGroupViewModel
    {
        public MenuItemPropertyGroup Model { get; set; }

        public MenuItemPropertyGroupViewModel(MenuItemPropertyGroup model)
        {
            Model = model;
            Properties = new List<MenuItemPropertyViewModel>(model.Properties.Select(x => new MenuItemPropertyViewModel(x)));
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
        public bool SingleSelection { get { return Model.SingleSelection; } set { Model.SingleSelection = value; } }
        public bool MultipleSelection { get { return Model.MultipleSelection; } set { Model.MultipleSelection = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int TerminalButtonHeight { get { return Model.TerminalButtonHeight; } set { Model.TerminalButtonHeight = value; } }
        public int TerminalColumnCount { get { return Model.TerminalColumnCount; } set { Model.TerminalColumnCount = value; } }

        public IList<MenuItemPropertyViewModel> Properties { get; set; }

        public void Refresh()
        {
            foreach (var model in Properties)
            {
                model.Refresh();
            }
        }
    }
}
