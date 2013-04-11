using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.UserModule
{
    public class PermissionViewModel
    {
        private readonly Permission _permission;
        public PermissionViewModel(Permission permission)
        {
            _permission = permission;
        }

        public string Title { get { return PermissionRegistry.PermissionNames[_permission.Name][1]; } }
        public string Category { get { return PermissionRegistry.PermissionNames[_permission.Name][0]; } }
        private static readonly string[] _values = new[] { Resources.Yes, Resources.No };
        public static string[] Values { get { return _values; } }
        public string Value { get { return Values[_permission.Value]; } set { _permission.Value = Values.ToList().IndexOf(value); } }
        public bool IsPermitted
        {
            get { return _permission.Value == (int)PermissionValue.Enabled; }
            set
            {
                if (value)
                    _permission.Value = (int)PermissionValue.Enabled;
                else
                    _permission.Value = (int)PermissionValue.Disabled;
            }
        }
    }
}
