using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Persistance.DaoClasses.Implementations
{
    [Export(typeof(IAutomationDao))]
    class AutomationDao : IAutomationDao
    {
        [ImportingConstructor]
        public AutomationDao()
        {
            ValidatorRegistry.RegisterDeleteValidator<AppAction>(x => Dao.Exists<ActionContainer>(y => y.AppActionId == x.Id), Resources.Action, Resources.Rule);
        }

        public Dictionary<string, string> GetScripts()
        {
            return Dao.Query<Script>().ToDictionary(x => x.HandlerName, x => x.Code);
        }

        public IEnumerable<AppRule> GetRules()
        {
            return Dao.Query<AppRule>(x => x.Actions, x => x.AppRuleMaps).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<AppAction> GetActions()
        {
            return Dao.Query<AppAction>().OrderBy(x => x.SortOrder);
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
