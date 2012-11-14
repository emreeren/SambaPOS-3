using System;
using System.Windows.Data;
using System.Globalization;

namespace Samba.Presentation.Controls.DataGridFilterLibrary.Support
{
    public class MyBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return System.Windows.Visibility.Visible;
            }
            else
            {
                return System.Windows.Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Visibility visibility = (System.Windows.Visibility)value;

            return visibility == System.Windows.Visibility.Visible ? true : false;
        }
    }
}
