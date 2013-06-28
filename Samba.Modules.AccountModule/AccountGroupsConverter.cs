using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Samba.Infrastructure.Settings;

namespace Samba.Modules.AccountModule
{
    public class AccountGroupsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
                return "null";

            var items = (ReadOnlyObservableCollection<object>)value;
            var balance = items.Cast<AccountRowData>().Sum(x => x.Balance);
            return balance.ToString(LocalSettings.ReportCurrencyFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}