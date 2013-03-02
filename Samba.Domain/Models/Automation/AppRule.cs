using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Automation
{
    public class AppRule : EntityClass, IOrderable
    {
        public string EventName { get; set; }
        public string EventConstraints { get; set; }
        public string CustomConstraint { get; set; }

        private readonly IList<ActionContainer> _actions;
        public virtual IList<ActionContainer> Actions
        {
            get { return _actions; }
        }

        private readonly IList<AppRuleMap> _appRuleMaps;
        public virtual IList<AppRuleMap> AppRuleMaps
        {
            get { return _appRuleMaps; }
        }

        public AppRule()
        {
            _actions = new List<ActionContainer>();
            _appRuleMaps = new List<AppRuleMap>();
        }

        public int SortOrder { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        public void AddRuleMap()
        {
            AppRuleMaps.Add(new AppRuleMap());
        }
    }
}
