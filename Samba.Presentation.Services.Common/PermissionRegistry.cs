using System.Collections.Generic;

namespace Samba.Presentation.Services.Common
{
    public static class PermissionRegistry
    {
        public static IDictionary<string, string[]> PermissionNames = new Dictionary<string, string[]>();

        public static void RegisterPermission(string permissionName, string permissionCategory, string permissionTitle)
        {
            if (!PermissionNames.ContainsKey(permissionName))
                PermissionNames.Add(permissionName, new[] { permissionCategory, permissionTitle });
        }
    }
}