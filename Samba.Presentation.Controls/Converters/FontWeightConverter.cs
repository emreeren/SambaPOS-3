using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Samba.Presentation.Controls.Converters
{
    public class FontWeightConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var param = parameter != null ? bool.Parse(parameter as string) : true;
            var val = (bool)value;
            return val == param ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var weight = (FontWeight)value;
            return (weight == FontWeights.Bold);
        }
    }
}
