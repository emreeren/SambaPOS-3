using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Automation
{
    public class AutomationCommand : EntityClass, IOrderable
    {
        public AutomationCommand()
        {
            _automationCommandMaps = new List<AutomationCommandMap>();
            FontSize = 30;
        }

        public string ButtonHeader { get; set; }
        public string Color { get; set; }
        public int FontSize { get; set; }
        public string Values { get; set; }
        public bool ToggleValues { get; set; }
        public int SortOrder { get; set; }

        private IList<AutomationCommandMap> _automationCommandMaps;
        public virtual IList<AutomationCommandMap> AutomationCommandMaps
        {
            get { return _automationCommandMaps; }
            set { _automationCommandMaps = value; }
        }

        public string UserString { get { return Name; } }
    }
}
