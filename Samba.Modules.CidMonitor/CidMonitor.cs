using System;
using System.Linq;
using Axcidv5callerid;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Entities;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.CidMonitor
{
    [ModuleExport(typeof(CidMonitor))]
    public class CidMonitor : ModuleBase
    {


    }
}
