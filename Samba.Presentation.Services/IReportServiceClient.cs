using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;

namespace Samba.Presentation.Services
{
    public interface IReportServiceClient
    {
        void PrintAccountScreen(AccountScreen accountScreen);
        void PrintAccountTransactions(Account account,string filter);
    }
}
