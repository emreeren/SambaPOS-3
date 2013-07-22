using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [Export]
    public class AccountSelectorViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IPrinterService _printerService;
        private AccountScreen _selectedAccountScreen;

        public event EventHandler Refreshed;

        protected virtual void OnRefreshed()
        {
            EventHandler handler = Refreshed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public ICaptionCommand ShowAccountDetailsCommand { get; set; }
        public ICaptionCommand PrintCommand { get; set; }
        public ICaptionCommand AccountButtonSelectedCommand { get; set; }

        [ImportingConstructor]
        public AccountSelectorViewModel(IAccountService accountService, ICacheService cacheService, IApplicationState applicationState,
            IPrinterService printerService)
        {
            _accounts = new ObservableCollection<AccountScreenRowModel>();
            _accountService = accountService;
            _cacheService = cacheService;
            _applicationState = applicationState;
            _printerService = printerService;
            ShowAccountDetailsCommand = new CaptionCommand<string>(Resources.AccountDetails.Replace(' ', '\r'), OnShowAccountDetails, CanShowAccountDetails);
            PrintCommand = new CaptionCommand<string>(Resources.Print, OnPrint);
            AccountButtonSelectedCommand = new CaptionCommand<AccountScreen>("", OnAccountScreenSelected);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
            x =>
            {
                if (x.Topic == EventTopicNames.ResetCache)
                {
                    _accountButtons = null;
                    _batchDocumentButtons = null;
                    _selectedAccountScreen = null;
                }
            });
        }

        private IEnumerable<DocumentTypeButtonViewModel> _batchDocumentButtons;
        public IEnumerable<DocumentTypeButtonViewModel> BatchDocumentButtons
        {
            get
            {
                return _batchDocumentButtons ??
                    (_batchDocumentButtons =
                    _selectedAccountScreen != null
                    ? _applicationState.GetBatchDocumentTypes(_selectedAccountScreen.AccountScreenValues.Select(x => x.AccountTypeName))
                            .Where(x => !string.IsNullOrEmpty(x.ButtonHeader))
                            .Select(x => new DocumentTypeButtonViewModel(x, null)) : null);
            }
        }

        public IEnumerable<AccountScreen> AccountScreens
        {
            get { return _cacheService.GetAccountScreens(); }
        }

        private IEnumerable<AccountButton> _accountButtons;
        public IEnumerable<AccountButton> AccountButtons
        {
            get { return _accountButtons ?? (_accountButtons = AccountScreens.Select(x => new AccountButton(x, _cacheService))); }
        }

        private readonly ObservableCollection<AccountScreenRowModel> _accounts;
        public ObservableCollection<AccountScreenRowModel> Accounts
        {
            get { return _accounts; }
        }

        public AccountScreenRowModel SelectedAccount { get; set; }


        private void OnAccountScreenSelected(AccountScreen accountScreen)
        {
            UpdateAccountScreen(accountScreen);
        }

        private bool CanShowAccountDetails(string arg)
        {
            return SelectedAccount != null && SelectedAccount.AccountId > 0;
        }

        private void OnShowAccountDetails(object obj)
        {
            CommonEventPublisher.PublishEntityOperation(new AccountData(SelectedAccount.AccountId), EventTopicNames.DisplayAccountTransactions, EventTopicNames.ActivateAccountSelector);
        }

        private void UpdateAccountScreen(AccountScreen accountScreen)
        {
            if (accountScreen == null) return;
            _batchDocumentButtons = null;
            _selectedAccountScreen = accountScreen;

            _accounts.Clear();
            _accounts.AddRange(_accountService.GetAccountScreenRows(accountScreen, _applicationState.CurrentWorkPeriod));

            RaisePropertyChanged(() => BatchDocumentButtons);
            RaisePropertyChanged(() => AccountButtons);

            OnRefreshed();
        }
        
        public void Refresh()
        {
            UpdateAccountScreen(_selectedAccountScreen ?? (_selectedAccountScreen = AccountScreens.FirstOrDefault()));
        }

        private void OnPrint(string obj)
        {
            var report = new SimpleReport("");
            report.AddParagraph("Header");
            report.AddParagraphLine("Header", string.Format(_selectedAccountScreen.Name), true);
            report.AddParagraphLine("Header", "");

            report.AddColumnLength("Transactions", "60*", "40*");
            report.AddColumTextAlignment("Transactions", TextAlignment.Left, TextAlignment.Right);
            report.AddTable("Transactions", string.Format(Resources.Name_f, Resources.Account), Resources.Balance);

            foreach (var ad in Accounts)
            {
                report.AddRow("Transactions", ad.Name, ad.BalanceStr);
            }

            _printerService.PrintReport(report.Document, _applicationState.GetReportPrinter());
        }
    }
}
