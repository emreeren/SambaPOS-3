using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Actions
{
    public class AppRule : Entity, IOrderable
    {
        public string EventName { get; set; }
        [StringLength(500)]
        public string EventConstraints { get; set; }

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

        public int Order { get; set; }

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
