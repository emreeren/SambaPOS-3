using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;

namespace Samba.Presentation.ViewModels
{
    public class OrderTagGroupViewModel
    {
        public OrderTagGroup Model { get; set; }

        public OrderTagGroupViewModel(OrderTagGroup model)
        {
            Model = model;
            OrderTags = new List<OrderTagViewModel>(model.OrderTags.Select(x => new OrderTagViewModel(x)));
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int TerminalButtonHeight { get { return Model.TerminalButtonHeight; } set { Model.TerminalButtonHeight = value; } }
        public int TerminalColumnCount { get { return Model.TerminalColumnCount; } set { Model.TerminalColumnCount = value; } }

        public IList<OrderTagViewModel> OrderTags { get; set; }

        public void Refresh()
        {
            foreach (var model in OrderTags)
            {
                model.Refresh();
            }
        }
    }
}
