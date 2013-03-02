using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;

namespace Samba.Modules.ModifierModule
{
    public class GroupedOrderTagViewModel
    {
        public string Name { get; set; }
        public IEnumerable<GroupedOrderTagButtonViewModel> OrderTags { get; set; }
        public int ColumnCount { get; set; }
        public int ButtonHeight { get; set; }

        public GroupedOrderTagViewModel(Order selectedItem, IGrouping<string, OrderTagGroup> orderTagGroups)
        {
            Name = orderTagGroups.Key;
            OrderTags = orderTagGroups.Select(x => new GroupedOrderTagButtonViewModel(selectedItem, x)).ToList();
            ColumnCount = orderTagGroups.First().ColumnCount;
            ButtonHeight = orderTagGroups.First().ButtonHeight;
        }
    }
}