using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Dashboards;
using Samba.Infrastructure.Data;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.DashboardModule
{
    public class DashboardMapViewModel : AbstractMapViewModel
    {
        public DashboardMap Model { get; set; }

        public DashboardMapViewModel(DashboardMap model, IUserService userService, IDepartmentService departmentService, ISettingService settingService)
            : base(model, userService, departmentService, settingService)
        {
            Model = model;
        }
    }
}
