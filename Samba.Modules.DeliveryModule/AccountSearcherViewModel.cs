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
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.DeliveryModule
{
    [Export]
    public class AccountSearcherViewModel : ObservableObject
    {
        [ImportingConstructor]
        public AccountSearcherViewModel()
        {
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;

            FoundAccounts = new ObservableCollection<AccountSearchViewModel>();
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
