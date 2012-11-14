using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Samba.Presentation.Controls.FxButton
{
    public class FontSizeToViewBoxDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value > 16 ? StretchDirection.DownOnly : StretchDirection.Both;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
