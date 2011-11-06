using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.UserModule
{
    [ModuleExport(typeof(UserModule))]
    public class UserModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        [ImportingConstructor]
        public UserModule(IRegionManager regionManager)
        {
            AddDashboardCommand<UserRoleListViewModel>(Resources.UserRoleList, Resources.Users, 50);
            AddDashboardCommand<UserListViewModel>(Resources.UserList, Resources.Users);
            _regionManager = regionManager;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.RightUserRegion, typeof(LoggedInUserView));
        }
    }
}
