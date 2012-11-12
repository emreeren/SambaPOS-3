using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Automation;

namespace Samba.Services
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
