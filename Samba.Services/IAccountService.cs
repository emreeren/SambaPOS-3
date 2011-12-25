using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Services
{
    public interface IAccountService
    {
        int GetAccountCount();
        bool DidAccountTemplateUsed(int accountTemplateId);
    }
}
