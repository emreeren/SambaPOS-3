using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Modules.AutomationModule
{
    [ModuleExport(typeof(AutomationModule))]
    class AutomationModule : ModuleBase
    {
        [ImportingConstructor]
        public AutomationModule()
        {
            AddDashboardCommand<RuleActionListViewModel>(Resources.RuleActions, Resources.Settings, 20);
            AddDashboardCommand<RuleListViewModel>(Resources.Rules, Resources.Settings, 20);
            AddDashboardCommand<TriggerListViewModel>(Resources.Triggers, Resources.Settings, 20);
        }
    }
}
