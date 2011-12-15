using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Services
{
    public interface IUserService : IService
    {
        string GetUserName(int userId);
        IEnumerable<string> GetUserNames();
        bool ContainsUser(int userId);
        bool IsDefaultUserConfigured { get; }
    }
}
