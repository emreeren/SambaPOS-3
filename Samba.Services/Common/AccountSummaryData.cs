using Samba.Infrastructure.Settings;

namespace Samba.Services.Common
{
    public class AccountSummaryData 
    {
        public AccountSummaryData(string caption, decimal debit, decimal credit)
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
            get { return _debit.ToString(LocalSettings.ReportCurrencyFormat); }
        }

        private readonly decimal _credit;
        public string Credit
        {
            get { return _credit.ToString(LocalSettings.ReportCurrencyFormat); }
        }

        public string Balance { get { return (_debit - _credit).ToString(LocalSettings.ReportCurrencyFormat); } }
    }
}
