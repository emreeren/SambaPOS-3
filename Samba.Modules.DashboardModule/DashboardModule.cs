using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Presentation.Common;

namespace Samba.Modules.DashboardModule
{
    [ModuleExport(typeof(DashboardModule))]
    class DashboardModule:ModuleBase
    {
    }
}
