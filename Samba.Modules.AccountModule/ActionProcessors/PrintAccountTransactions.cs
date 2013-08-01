using System.ComponentModel.Composition;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class PrintAccountTransactions : ActionType
    {
        private readonly IAccountService _accountService;
        private readonly IReportServiceClient _reportServiceClient;

        [ImportingConstructor]
        public PrintAccountTransactions(IAccountService accountService, IReportServiceClient reportServiceClient)
        {
            _accountService = accountService;
            _reportServiceClient = reportServiceClient;
        }

        public override void Process(ActionData actionData)
        {
            var accountName = actionData.GetAsString("AccountName");
            if (!string.IsNullOrEmpty(accountName))
            {
                var account = _accountService.GetAccountByName(accountName);
                if (account != null)
                {
                    _reportServiceClient.PrintAccountTransactions(account);
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new { AccountName = "" };
        }

        protected override string GetActionName()
        {
            return Resources.PrintAccountTransactions;
        }

        protected override string GetActionKey()
        {
            return "PrintAccountTransactions";
        }
    }
}
