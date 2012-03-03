using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
{
    [Export]
    public class AccountSelectorViewModel : ObservableObject
    {
        public event EventHandler SelectedAccountTemplateChanged;

        private void InvokeSelectedAccountTemplateChanged(EventArgs e)
        {
            var handler = SelectedAccountTemplateChanged;
            if (handler != null) handler(this, e);
        }

        private EntityOperationRequest<Account> _currentAccountSelectionRequest;

        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand SelectAccountCommand { get; set; }
        public ICaptionCommand CreateAccountCommand { get; set; }
        public ICaptionCommand EditAccountCommand { get; set; }
        public ICaptionCommand DisplayAccountCommand { get; set; }

        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountSelectorViewModel(IApplicationState applicationState, ICacheService cacheService)
        {
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;

            _applicationState = applicationState;
            _cacheService = cacheService;

            FoundAccounts = new ObservableCollection<AccountSearchViewModel>();

            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreen);
            SelectAccountCommand = new CaptionCommand<string>(Resources.SelectAccount.Replace(" ", "\r"), OnSelectAccount, CanSelectAccount);
            EditAccountCommand = new CaptionCommand<string>(string.Format(Resources.Edit_f, Resources.Account).Replace(" ", "\r"), OnEditAccount, CanEditAccount);
            CreateAccountCommand = new CaptionCommand<string>(Resources.NewAccount.Replace(" ", "\r"), OnCreateAccount, CanCreateAccount);
            DisplayAccountCommand = new CaptionCommand<string>(Resources.AccountDetails.Replace(" ", "\r"), OnDisplayAccount, CanDisplayAccount);
        }

        public IEnumerable<AccountTemplate> AccountTemplates { get { return _cacheService.GetAccountTemplates(); } }
        private AccountTemplate _selectedAccountTemplate;
        public AccountTemplate SelectedAccountTemplate
        {
            get { return _selectedAccountTemplate; }
            set
            {
                _selectedAccountTemplate = value;
                ClearSearchValues();
                RaisePropertyChanged(() => SelectedAccountTemplate);
                InvokeSelectedAccountTemplateChanged(EventArgs.Empty);
            }
        }

        private readonly Timer _updateTimer;
        public ObservableCollection<AccountSearchViewModel> FoundAccounts { get; set; }

        public AccountSearchViewModel SelectedAccount
        {
            get
            {
                return FoundAccounts.Count == 1 ? FoundAccounts[0] : FocusedAccount;
            }
        }

        private AccountSearchViewModel _focusedAccount;
        public AccountSearchViewModel FocusedAccount
        {
            get { return _focusedAccount; }
            set
            {
                _focusedAccount = value;
                RaisePropertyChanged(() => FocusedAccount);
                RaisePropertyChanged(() => SelectedAccount);
            }
        }

        private string _searchString;
        public string SearchString
        {
            get { return string.IsNullOrEmpty(_searchString) ? null : _searchString.TrimStart('+', '0'); }
            set
            {
                if (value != _searchString)
                {
                    _searchString = value;
                    RaisePropertyChanged(() => SearchString);
                    ResetTimer();
                }
            }
        }

        public bool IsCloseButtonVisible { get { return _applicationState.CurrentDepartment != null; } }

        private void OnDisplayAccount(string obj)
        {
            CommonEventPublisher.PublishEntityOperation(SelectedAccount.Model, EventTopicNames.DisplayAccountTransactions);
            ClearSearchValues();
        }

        private bool CanEditAccount(string arg)
        {
            return SelectedAccount != null;
        }

        private void OnEditAccount(string obj)
        {
            CommonEventPublisher.PublishEntityOperation(SelectedAccount.Model, EventTopicNames.EditAccountDetails, EventTopicNames.AccountSelected);
        }

        private bool CanCreateAccount(string arg)
        {
            return SelectedAccountTemplate != null;
        }

        private void OnCreateAccount(string obj)
        {
            ClearSearchValues();
            CommonEventPublisher.PublishEntityOperation(new Account { AccountTemplateId = SelectedAccountTemplate.Id }, EventTopicNames.EditAccountDetails, EventTopicNames.AccountSelected);
        }

        private bool CanSelectAccount(string arg)
        {
            return
                _applicationState.IsCurrentWorkPeriodOpen
                && _applicationState.CurrentDepartment != null
                && SelectedAccount != null
                && !string.IsNullOrEmpty(SelectedAccount.Name);
        }

        private bool CanDisplayAccount(string arg)
        {
            return SelectedAccount != null;
        }

        private void OnSelectAccount(string obj)
        {
            if (_currentAccountSelectionRequest != null)
            {
                _currentAccountSelectionRequest.Publish(SelectedAccount.Model);
            }
            CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateOpenTickets);
            ClearSearchValues();
        }

        private void OnCloseScreen(string obj)
        {
            CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateOpenTickets);
        }

        public void RefreshSelectedAccount(EntityOperationRequest<Account> value)
        {
            if (_applicationState.CurrentDepartment != null)
            {
                if (SelectedAccountTemplate == null || SelectedAccountTemplate.Id != _applicationState.CurrentDepartment.TicketTemplate.SaleTransactionTemplate.TargetAccountTemplateId)
                    SelectedAccountTemplate = _cacheService.GetAccountTemplateById(
                            _applicationState.CurrentDepartment.TicketTemplate.SaleTransactionTemplate.TargetAccountTemplateId);

                ClearSearchValues();

                _currentAccountSelectionRequest = value;

                if (_currentAccountSelectionRequest != null)
                {
                    ClearSearchValues();
                    FoundAccounts.Add(new AccountSearchViewModel(_currentAccountSelectionRequest.SelectedEntity, SelectedAccountTemplate));
                }
            }

            RaisePropertyChanged(() => SelectedAccountTemplate);
            RaisePropertyChanged(() => SelectedAccount);
            RaisePropertyChanged(() => IsCloseButtonVisible);
            RaisePropertyChanged(() => AccountTemplates);
        }

        private void ClearSearchValues()
        {
            FoundAccounts.Clear();
            SearchString = "";
        }

        private void ResetTimer()
        {
            _updateTimer.Stop();

            if (!string.IsNullOrEmpty(SearchString))
            {
                _updateTimer.Start();
            }
            else FoundAccounts.Clear();
        }

        void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _updateTimer.Stop();
            UpdateFoundAccounts();
        }

        private void UpdateFoundAccounts()
        {
            IEnumerable<Account> result = new List<Account>();

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                {
                    var defaultAccountId =
                        _applicationState.CurrentDepartment != null ? _applicationState.CurrentDepartment.TicketTemplate.SaleTransactionTemplate.DefaultTargetAccountId : 0;

                    var templateId = SelectedAccountTemplate != null ? SelectedAccountTemplate.Id : 0;

                    result = Dao.Query<Account>(x =>
                        x.AccountTemplateId == templateId
                        && x.Id != defaultAccountId
                        && (x.CustomData.Contains(SearchString) || x.Name.Contains(SearchString)));

                    result = result.ToList().Where(x => SelectedAccountTemplate.GetMatchingFields(x, SearchString).Any(y => !y.Hidden) || x.Name.ToLower().Contains(SearchString));
                };

                worker.RunWorkerCompleted +=
                    delegate
                    {

                        AppServices.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                               delegate
                               {
                                   FoundAccounts.Clear();
                                   FoundAccounts.AddRange(result.Select(x => new AccountSearchViewModel(x, SelectedAccountTemplate)));

                                   if (SelectedAccount != null && SearchString == SelectedAccount.PhoneNumber)
                                   {
                                       SelectedAccount.UpdateDetailedInfo();
                                   }

                                   RaisePropertyChanged(() => SelectedAccount);
                                   CommandManager.InvalidateRequerySuggested();
                                   SelectedAccount.PublishEvent(EventTopicNames.SelectedAccountChanged);
                               }));

                    };

                worker.RunWorkerAsync();
            }
        }

    }
}
