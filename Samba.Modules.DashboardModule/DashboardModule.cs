using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Dashboards;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.DashboardModule
{
    [ModuleExport(typeof(DashboardModule))]
    class DashboardModule : ModuleBase
    {
        [ImportingConstructor]
        public DashboardModule()
        {
            AddDashboardCommand<EntityCollectionViewModelBase<DashboardViewModel, Dashboard>>(string.Format(Resources.List_f, Resources.Dashboard), Resources.Settings);
        }
    }
}
