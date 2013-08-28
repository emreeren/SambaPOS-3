using System;
using System.Windows.Data;
using System.Windows.Media;

namespace FastButton
{
    public class ColorToAlphaColorConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            var brush = (SolidColorBrush)value;
                        
            if( brush != null )
            {
                var color = brush.Color;
                color.A = byte.Parse( parameter.ToString() );
                return color;
            }

            return Colors.Black; // make error obvious
        }


        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
