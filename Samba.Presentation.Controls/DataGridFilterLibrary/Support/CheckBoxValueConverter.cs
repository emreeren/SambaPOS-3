using System;
using System.Windows.Data;

namespace Samba.Presentation.Controls.DataGridFilterLibrary.Support
{
    public class CheckBoxValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;

            if (value != null && value.GetType() == typeof(String))
            {
                Boolean.TryParse(value.ToString(), out result);
            }
            else if (value != null)
            {
                result = System.Convert.ToBoolean(value);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}
