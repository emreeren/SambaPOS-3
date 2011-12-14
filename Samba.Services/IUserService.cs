using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Services
{
    public interface IUserService : IService
    {
        string GetUserName(int userId);
    }
}
