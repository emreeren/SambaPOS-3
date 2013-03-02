﻿using System;
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
        public CidMonitor()
        {
            try
            {
                var frmMain = new FrmMain();
                frmMain.axCIDv51.OnCallerID += axCIDv51_OnCallerID;
                frmMain.axCIDv51.Start();
            }
            catch (Exception)
            {
#if DEBUG
                var i = 0;
#else
                InteractionService.UserIntraction.DisplayPopup(Resources.Information, Resources.CallerIdDriverError, "", "");
#endif
            }
        }

        static void axCIDv51_OnCallerID(object sender, ICIDv5Events_OnCallerIDEvent e)
        {
            var pn = e.phoneNumber;
            pn = pn.TrimStart('+');
            pn = pn.TrimStart('0');
            pn = pn.TrimStart('9');
            pn = pn.TrimStart('0');

            var c = Dao.Query<Entity>(x => x.SearchString == pn).ToList();
            if (!c.Any())
                c = Dao.Query<Entity>(x => x.SearchString.Contains(pn)).ToList();
            if (c.Count() == 1)
            {
                var entity = c.First();
                InteractionService.UserIntraction.DisplayPopup(entity.Name, entity.Name + " " + Resources.Calling + ".\r" + entity.SearchString + "\r",
                                                            entity.SearchString, EventTopicNames.SelectEntity);
            }
            else
                InteractionService.UserIntraction.DisplayPopup(e.phoneNumber, e.phoneNumber + " " + Resources.Calling + "...",
                                                               e.phoneNumber, EventTopicNames.SelectEntity);
        }
    }
}
