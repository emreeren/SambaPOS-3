using System.Collections.Generic;
using System.Linq;

namespace Samba.Presentation.Common
{
    public class DashboardCommandCategory
    {
        private readonly List<ICategoryCommand> _commands;
        public int Order { get; set; }
        public string Category { get; set; }
        public IEnumerable<ICategoryCommand> Commands { get { return _commands.OrderBy(x => x.Order); } }

        public DashboardCommandCategory(string category)
        {
            Category = category;
            _commands=new List<ICategoryCommand>();
        }

        public void AddCommand(ICategoryCommand command)
        {
            _commands.Add(command);
        }
    }
}
