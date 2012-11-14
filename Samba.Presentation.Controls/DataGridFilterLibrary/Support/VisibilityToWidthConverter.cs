using System;
using System.Windows.Data;
using System.Globalization;

namespace Samba.Presentation.Controls.DataGridFilterLibrary.Support
{
    public class VisibilityToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Visibility visibility = (System.Windows.Visibility)value;

            return visibility == System.Windows.Visibility.Visible ? Double.NaN : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
