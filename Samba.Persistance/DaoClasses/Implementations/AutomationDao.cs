using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Persistance.Data;

namespace Samba.Persistance.DaoClasses.Implementations
{
    [Export(typeof(IAutomationDao))]
    class AutomationDao : IAutomationDao
    {
        public Dictionary<string, string> GetScripts()
        {
            return Dao.Query<Script>().ToDictionary(x => x.HandlerName, x => x.Code);
        }

        public IEnumerable<AppRule> GetRules()
        {
            return Dao.Query<AppRule>(x => x.Actions, x => x.AppRuleMaps).OrderBy(x => x.Order);
        }

        public IEnumerable<AppAction> GetActions()
        {
            return Dao.Query<AppAction>().OrderBy(x => x.Order);
        }

        public AppAction GetActionById(int appActionId)
        {
            return Dao.Single<AppAction>(x => x.Id == appActionId);
        }

        public IEnumerable<string> GetAutomationCommandNames()
        {
            return Dao.Distinct<AutomationCommand>(x => x.Name);
        }
    }
}
