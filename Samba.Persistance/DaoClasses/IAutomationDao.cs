using System.Collections.Generic;
using Samba.Domain.Models.Automation;

namespace Samba.Persistance.DaoClasses
{
    public interface IAutomationDao
    {
        Dictionary<string, string> GetScripts();
        IEnumerable<AppRule> GetRules();
        IEnumerable<AppAction> GetActions();
        AppAction GetActionById(int appActionId);
        IEnumerable<string> GetAutomationCommandNames();
    }
}
