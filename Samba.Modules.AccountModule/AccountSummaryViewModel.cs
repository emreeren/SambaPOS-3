using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule
{
    public class AccountSummaryViewModel : ObservableObject
    {
        public AccountSummaryViewModel(string caption, decimal debit, decimal credit)
        {
            _caption = caption;
            _debit = debit;
            _credit = credit;
        }

        private readonly string _caption;
        public string Caption
        {
            get { return _caption; }
        }

        private readonly decimal _debit;
        public string Debit
        {
            get { return _debit.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        private readonly decimal _credit;
        public string Credit
        {
            get { return _credit.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string Balance { get { return (_debit - _credit).ToString(LocalSettings.DefaultCurrencyFormat); } }
    }
}
