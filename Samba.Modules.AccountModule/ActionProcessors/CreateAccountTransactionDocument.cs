using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class CreateAccountTransactionDocument : ActionType
    {
        private readonly ICacheService _cacheService;
        private readonly IAccountService _accountService;

        [ImportingConstructor]
        public CreateAccountTransactionDocument(ICacheService cacheService, IAccountService accountService)
        {
            _cacheService = cacheService;
            _accountService = accountService;
        }

        protected override object GetDefaultData()
        {
            return new { AccountTransactionDocumentName = "", AccountName = "", Description = "", Amount = 0m };
        }

        protected override string GetActionName()
        {
            return string.Format(Resources.Create_f, Resources.AccountTransactionDocument);
        }

        protected override string GetActionKey()
        {
            return ActionNames.CreateAccountTransactionDocument;
        }

        public override void Process(ActionData actionData)
        {
            var documentName = actionData.GetAsString("AccountTransactionDocumentName");
            var documentType = _cacheService.GetAccountTransactionDocumentTypeByName(documentName);
            var accountName = actionData.GetAsString("AccountName");
            var description = actionData.GetAsString("Description");
            var amount = actionData.GetAsDecimal("Amount");
            if (amount > 0)
            {
                var account = _accountService.GetAccountByName(accountName);
                var document = _accountService.CreateTransactionDocument(account, documentType, description, amount, null);
                actionData.DataObject.DocumentId = document.Id;
            }
        }
    }
}
