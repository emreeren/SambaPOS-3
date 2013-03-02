using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Samba.Presentation.Controls.Converters
{
    public class NullBrushConverter : IValueConverter
    {
        private readonly BrushConverter _brushConverter = new BrushConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == "")
                return DependencyProperty.UnsetValue;
            return _brushConverter.ConvertFromString(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
