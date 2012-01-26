using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
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

namespace Samba.Modules.DeliveryModule
{
    [Export]
    public class AccountSelectorViewModel : ObservableObject
    {
        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand SelectAccountCommand { get; set; }
        public ICaptionCommand CreateAccountCommand { get; set; }
        public ICaptionCommand ResetAccountCommand { get; set; }
        public ICaptionCommand FindTicketCommand { get; set; }
        public ICaptionCommand MakePaymentCommand { get; set; }
        public ICaptionCommand DisplayAccountCommand { get; set; }

        private readonly IApplicationState _applicationState;
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;

        [ImportingConstructor]
        public AccountSelectorViewModel(IApplicationState applicationState, ITicketService ticketService, IUserService userService)
        {
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;

            _applicationState = applicationState;
            _ticketService = ticketService;
            _userService = userService;

            FoundAccounts = new ObservableCollection<AccountSearchViewModel>();

            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreen);
            SelectAccountCommand = new CaptionCommand<string>(Resources.SelectAccount_r, OnSelectAccount, CanSelectAccount);
            CreateAccountCommand = new CaptionCommand<string>(Resources.NewAccount_r, OnCreateAccount, CanCreateAccount);
            FindTicketCommand = new CaptionCommand<string>(Resources.FindTicket_r, OnFindTicket, CanFindTicket);
            ResetAccountCommand = new CaptionCommand<string>(Resources.ResetAccount_r, OnResetAccount, CanResetAccount);
            MakePaymentCommand = new CaptionCommand<string>(Resources.GetPayment_r, OnMakePayment, CanMakePayment);
            DisplayAccountCommand = new CaptionCommand<string>(Resources.Account, OnDisplayAccount, CanSelectAccount);
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

        private string _ticketSearchText;
        public string TicketSearchText
        {
            get { return _ticketSearchText; }
            set { _ticketSearchText = value; RaisePropertyChanged(() => TicketSearchText); }
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

        public bool IsResetAccountVisible
        {
            get
            {
                return (_applicationState.CurrentTicket != null &&
                        _applicationState.CurrentTicket.AccountId > 0);
            }
        }

        public bool IsClearVisible
        {
            get
            {
                return (_applicationState.CurrentTicket != null &&
                        _applicationState.CurrentTicket.AccountId == 0);
            }
        }

        public bool IsMakePaymentVisible
        {
            get
            {
                return (_applicationState.CurrentTicket != null && _userService.IsUserPermittedFor(PermissionNames.MakePayment));
            }
        }

        private bool CanMakePayment(string arg)
        {
            return SelectedAccount != null && _applicationState.CurrentTicket != null;
        }

        private void OnMakePayment(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.PaymentRequestedForTicket);
            ClearSearchValues();
        }

        private void OnDisplayAccount(string obj)
        {
            SelectedAccount.Model.PublishEvent(EventTopicNames.DisplayAccountTransactions);
        }

        private bool CanResetAccount(string arg)
        {
            return _applicationState.CurrentTicket != null &&
                _applicationState.CurrentTicket.CanSubmit &&
                _applicationState.CurrentTicket.AccountId > 0;
        }

        private static void OnResetAccount(string obj)
        {
            Account.Null.PublishEvent(EventTopicNames.AccountSelectedForTicket);
        }

        private void OnFindTicket(string obj)
        {
            _ticketService.OpenTicketByTicketNumber(TicketSearchText);
            if (_applicationState.CurrentTicket != null)
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
            TicketSearchText = "";
        }

        private bool CanFindTicket(string arg)
        {
            return !string.IsNullOrEmpty(TicketSearchText) && _applicationState.CurrentTicket == null;
        }

        private bool CanCreateAccount(string arg)
        {
            return SelectedAccount == null;
        }

        private void OnCreateAccount(string obj)
        {
            ClearSearchValues();
            var c = new Account();
            c.PublishEvent(EventTopicNames.EditAccountDetails);
        }

        private bool CanSelectAccount(string arg)
        {
            return
                _applicationState.IsCurrentWorkPeriodOpen
                && SelectedAccount != null
                && !string.IsNullOrEmpty(SelectedAccount.Name)
                && (_applicationState.CurrentTicket == null || _applicationState.CurrentTicket.AccountId == 0);
        }

        private void OnSelectAccount(string obj)
        {
            SaveSelectedAccount();
            SelectedAccount.Model.PublishEvent(EventTopicNames.AccountSelectedForTicket);
            ClearSearchValues();
        }

        private void OnCloseScreen(string obj)
        {
            _applicationState.CurrentDepartment.PublishEvent(EventTopicNames.ActivateOpenTickets);
        }

        public void RefreshSelectedAccount()
        {
            ClearSearchValues();

            if (_applicationState.CurrentTicket != null && _applicationState.CurrentTicket.AccountId > 0)
            {
                var account = Dao.SingleWithCache<Account>(x => x.Id == _applicationState.CurrentTicket.AccountId);
                if (account != null)
                {
                    ClearSearchValues();
                    account.PublishEvent(EventTopicNames.EditAccountDetails);
                }
                //    FocusedAccount = new AccountSearchViewModel(account);
                //if (SelectedAccount != null)
                //{
                //    SelectedAccount.UpdateDetailedInfo();
                //}
            }
            RaisePropertyChanged(() => SelectedAccount);
            RaisePropertyChanged(() => IsClearVisible);
            RaisePropertyChanged(() => IsResetAccountVisible);
            RaisePropertyChanged(() => IsMakePaymentVisible);
        }

        private void SaveSelectedAccount()
        {
            if (!SelectedAccount.IsNotNew)
            {
                var ws = WorkspaceFactory.Create();
                ws.Add(SelectedAccount.Model);
                ws.CommitChanges();
            }
        }

        private void ClearSearchValues()
        {
            FoundAccounts.Clear();
            SearchString = "";
            TicketSearchText = "";
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
                    var searchPn = string.IsNullOrEmpty(SearchString.Trim());
                    result = Dao.Query<Account>(
                        x => (searchPn || x.CustomData.Contains(SearchString) || x.Name.Contains(SearchString)));
                };

                worker.RunWorkerCompleted +=
                    delegate
                    {

                        AppServices.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                               delegate
                               {
                                   FoundAccounts.Clear();
                                   FoundAccounts.AddRange(result.Select(x => new AccountSearchViewModel(x)));

                                   if (SelectedAccount != null && SearchString == SelectedAccount.PhoneNumber)
                                   {
                                       //SelectedView = 1;
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
