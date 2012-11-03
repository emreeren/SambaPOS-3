using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Automation
{
    public class AutomationCommand : Entity, IOrderable
    {
        public AutomationCommand()
        {
            _automationCommandMaps = new List<AutomationCommandMap>();
        }

        public string ButtonHeader { get; set; }
        public string Color { get; set; }
        public string Values { get; set; }
        public bool ToggleValues { get; set; }
        public int Order { get; set; }

        private readonly IList<AutomationCommandMap> _automationCommandMaps;
        public virtual IList<AutomationCommandMap> AutomationCommandMaps
        {
            get { return _automationCommandMaps; }
        }

        public string UserString { get { return Name; } }
        
        public AutomationCommandMap AddAutomationCommandMap()
        {
            var map = new AutomationCommandMap();
            _automationCommandMaps.Add(map);
            return map;
        }
    }
}
