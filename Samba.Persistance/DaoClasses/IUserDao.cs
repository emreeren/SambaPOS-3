using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Users;

namespace Samba.Persistance.DaoClasses
{
    public interface IUserDao
    {
        bool GetIsUserExists(string pinCode);
        User GetUserByPinCode(string pinCode);
        IEnumerable<UserRole> GetUserRoles();
    }
}
