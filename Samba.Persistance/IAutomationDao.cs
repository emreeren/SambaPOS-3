using System.Collections.Generic;
using Samba.Domain.Models.Automation;

namespace Samba.Persistance
{
    public interface IAutomationDao
    {
        Dictionary<string, string> GetScripts();
        AppAction GetActionById(int appActionId);
        IEnumerable<string> GetAutomationCommandNames();
    }
}
