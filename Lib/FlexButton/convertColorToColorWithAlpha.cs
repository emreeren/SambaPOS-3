//        Another Demo from Andy L. & MissedMemo.com
// Borrow whatever code seems useful - just don't try to hold
// me responsible for any ill effects. My demos sometimes use
// licensed images which CANNOT legally be copied and reused.

using System;
using System.Windows.Data;
using System.Windows.Media;

namespace FlexButton
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
