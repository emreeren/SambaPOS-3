using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    public class DocumentTemplateButtonViewModel : ObservableObject
    {
        public DocumentTemplateButtonViewModel(AccountTransactionDocumentTemplate model, Account account)
        {
            Model = model;
            Account = account;
            SelectDocumentTemplateCommand = new DelegateCommand<string>(OnSelectDocumentTemplate);
        }

        public AccountTransactionDocumentTemplate Model { get; set; }
        public Account Account { get; set; }
        public DelegateCommand<string> SelectDocumentTemplateCommand { get; set; }

        public string ButtonHeader { get { return Model.ButtonHeader.Replace(" ", "\r"); } }
        public string ButtonColor { get { return Model.ButtonColor; } }

        private void OnSelectDocumentTemplate(string obj)
        {
            var creationData = new DocumentCreationData(Account, Model);
            creationData.PublishEvent(EventTopicNames.AccountTransactionDocumentSelected);
        }
    }
}
