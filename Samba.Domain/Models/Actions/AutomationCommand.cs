using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Actions
{
    public class AutomationCommand : Entity, IOrderable
    {
        public string ButtonHeader { get; set; }
        public string Color { get; set; }

        private readonly IList<AutomationCommandMap> _automationCommandMaps;
        public virtual IList<AutomationCommandMap> AutomationCommandMaps
        {
            get { return _automationCommandMaps; }
        }

        public AutomationCommand()
        {
            _automationCommandMaps = new List<AutomationCommandMap>();
        }

        public AutomationCommandMap AddAutomationCommandMap()
        {
            var map = new AutomationCommandMap();
            _automationCommandMaps.Add(map);
            return map;
        }

        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }
    }
}
