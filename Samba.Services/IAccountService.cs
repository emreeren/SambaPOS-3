using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;

namespace Samba.Services
{
    public interface IAccountService
    {
        int GetAccountCount();
    }
}
