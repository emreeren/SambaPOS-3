using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Users;
using Samba.Persistance.Data;
using Samba.Services;

namespace Samba.Modules.UserModule.ServiceImplementations
{
    [Export(typeof(IUserService))]
    public class UserService : IUserService
    {
        private IEnumerable<User> _users;
        public IEnumerable<User> Users { get { return _users ?? (_users = Dao.Query<User>(x => x.UserRole)); } }

        public string GetUserName(int userId)
        {
            return userId > 0 ? Users.Single(x => x.Id == userId).Name : "-";
        }

        public void Reset()
        {
            _users = null;
        }
    }
}
