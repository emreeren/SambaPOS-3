using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IUserService : IService
    {
        string GetUserName(int userId);
        IEnumerable<string> GetUserNames();
        IEnumerable<Department> PermittedDepartments { get; }
        bool ContainsUser(int userId);
        bool IsDefaultUserConfigured { get; }
        User LoginUser(string pinValue);
        void LogoutUser(bool resetCache = true);
        bool IsUserPermittedFor(string permissionName);
    }
}
