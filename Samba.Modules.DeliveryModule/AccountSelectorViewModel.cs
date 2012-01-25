using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Transactions;
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

        public AccountSearchViewModel SelectedAccount { get; set; }

        private int _selectedView;
        public int SelectedView
        {
            get { return _selectedView; }
            set { _selectedView = value; RaisePropertyChanged(() => SelectedView); }
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

        private int _activeView;
        private readonly IApplicationState _applicationState;
        private readonly IUserService _userService;
        private readonly ITicketService _ticketService;
        private readonly IRegionManager _regionManager;
        private readonly AccountTransactionsView _accountTransactionsView;
        private readonly AccountTransactionsViewModel _accountTransactionsViewModel;

        public int ActiveView
        {
            get { return _activeView; }
            set { _activeView = value; RaisePropertyChanged(() => ActiveView); }
        }

        [ImportingConstructor]
        public AccountSelectorViewModel(IRegionManager regionManager,
            IApplicationState applicationState, IUserService userService,
            AccountTransactionsView accountTransactionsView,AccountTransactionsViewModel accountTransactionsViewModel,
            ITicketService ticketService)
        {
            _applicationState = applicationState;
            _userService = userService;
            _ticketService = ticketService;
            _regionManager = regionManager;
            _accountTransactionsView = accountTransactionsView;
            _accountTransactionsViewModel = accountTransactionsViewModel;

            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountTransactionsView));

            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreen);
            SelectAccountCommand = new CaptionCommand<string>(Resources.SelectAccount_r, OnSelectAccount, CanSelectAccount);
            CreateAccountCommand = new CaptionCommand<string>(Resources.NewAccount_r, OnCreateAccount, CanCreateAccount);
            //FindTicketCommand = new CaptionCommand<string>(Resources.FindTicket_r, OnFindTicket, CanFindTicket);
            ResetAccountCommand = new CaptionCommand<string>(Resources.ResetAccount_r, OnResetAccount, CanResetAccount);
            MakePaymentCommand = new CaptionCommand<string>(Resources.GetPayment_r, OnMakePayment, CanMakePayment);
            DisplayAccountCommand = new CaptionCommand<string>(Resources.Account, OnDisplayAccount, CanSelectAccount);

            EventServiceFactory.EventService.GetEvent<GenericEvent<AccountSearchViewModel>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.SelectedAccountChanged)
                    {
                        SelectedAccount = x.Value;
                    }
                }
                );
        }

        internal void DisplayAccount(Account account)
        {
            SelectedAccount = new AccountSearchViewModel(account);
            RaisePropertyChanged(() => SelectedAccount);
            OnDisplayAccount("");
        }

        private void OnDisplayAccount(string obj)
        {
            //ActiveView = 1;
            SaveSelectedAccount();
            _accountTransactionsViewModel.SelectedAccount = SelectedAccount;
            _regionManager.Regions[RegionNames.MainRegion].Activate(_accountTransactionsView);
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

        //private void OnFindTicket(string obj)
        //{
        //    _ticketService.OpenTicketByTicketNumber(TicketSearchText);
        //    if (_applicationState.CurrentTicket != null)
        //        EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        //    TicketSearchText = "";
        //}

        //private bool CanFindTicket(string arg)
        //{
        //    return !string.IsNullOrEmpty(TicketSearchText) && _applicationState.CurrentTicket == null;
        //}

        private bool CanCreateAccount(string arg)
        {
            return SelectedAccount == null;
        }

        private void OnCreateAccount(string obj)
        {
            var c = new Account();
            SelectedAccount = new AccountSearchViewModel(c);
            SelectedView = 1;
            RaisePropertyChanged(() => SelectedAccount);
        }

        private bool CanSelectAccount(string arg)
        {
            return
                _applicationState.IsCurrentWorkPeriodOpen
                && SelectedAccount != null
                && !string.IsNullOrEmpty(SelectedAccount.Name)
                && (_applicationState.CurrentTicket == null || _applicationState.CurrentTicket.AccountId == 0);
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

        private void OnSelectAccount(string obj)
        {
            SaveSelectedAccount();
            SelectedAccount.Model.PublishEvent(EventTopicNames.AccountSelectedForTicket);
            ClearSearchValues();
        }

        private void OnCloseScreen(string obj)
        {
            _applicationState.CurrentDepartment.PublishEvent(EventTopicNames.ActivateOpenTickets);
            SelectedView = 0;
            ActiveView = 0;
            //SelectedAccountTransactions.Clear();
        }

        public void RefreshSelectedAccount()
        {
            ClearSearchValues();

            if (_applicationState.CurrentTicket != null && _applicationState.CurrentTicket.AccountId > 0)
            {
                var account = Dao.SingleWithCache<Account>(x => x.Id == _applicationState.CurrentTicket.AccountId);
                if (account != null)
                    SelectedAccount = new AccountSearchViewModel(account);
                //FoundAccounts.Add(new AccountSearchViewModel(account));
                if (SelectedAccount != null)
                {
                    SelectedView = 1;
                    SelectedAccount.UpdateDetailedInfo();
                }
            }
            RaisePropertyChanged(() => SelectedAccount);
            RaisePropertyChanged(() => IsClearVisible);
            RaisePropertyChanged(() => IsResetAccountVisible);
            RaisePropertyChanged(() => IsMakePaymentVisible);
            ActiveView = 0;
            //SelectedAccountTransactions.Clear();
        }

        private void ClearSearchValues()
        {
            //FoundAccounts.Clear();
            //SelectedView = 0;
            //ActiveView = 0;
            //SearchString = "";
            //AccountNameSearchText = "";
        }

        public void SearchAccount(string phoneNumber)
        {
            //ClearSearchValues();
            //SearchString = phoneNumber;
            //UpdateFoundAccounts();
        }
    }
}
