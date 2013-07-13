using System.Collections.Generic;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services
{
    public interface IUserService : IPresentationService
    {
        string GetUserName(int userId);
        IEnumerable<Department> PermittedDepartments { get; }
        bool ContainsUser(int userId);
        bool IsDefaultUserConfigured { get; }
        User LoginUser(string pinValue);
        void LogoutUser(bool resetCache = true);
        bool IsUserPermittedFor(string permissionName);
        IEnumerable<UserRole> GetUserRoles();
    }
}
