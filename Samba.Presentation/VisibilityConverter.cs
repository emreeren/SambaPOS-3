using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Samba.Presentation
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            //bool visibility = (bool)value;
            //return visibility ? Visibility.Visible : Visibility.Collapsed;

            bool param = parameter != null ? bool.Parse(parameter as string) : true;
            bool val = (bool)value;

            return val == param ?
              Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return (visibility == Visibility.Visible);
        }
    }



    //public class BoolToVisibilityConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType,
    //        object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        bool param = bool.Parse(parameter as string);
    //        bool val = (bool)value;

    //        return val == param ?
    //          Visibility.Visible : Visibility.Hidden;
    //    }

    //    public object ConvertBack(object value, Type targetType,
    //        object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
