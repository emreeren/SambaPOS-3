using System.Collections.Generic;
using Samba.Domain.Models.Users;

namespace Samba.Persistance
{
    public interface IUserDao
    {
        bool GetIsUserExists(string pinCode);
        User GetUserByPinCode(string pinCode);
        IEnumerable<UserRole> GetUserRoles();
    }
}
