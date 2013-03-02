/*
 *   Demo program for ListView Sorting
 *   Created by Li Gao, 2007
 *   
 *   Modified for demo purposes only.
 *   This program is provided as is and no warranty or support.
 *   Use it as your own risk
 * */

using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;

namespace Samba.Presentation.Controls.ListViewEx
{
    public sealed class BackgroundConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            ListViewItem item = (ListViewItem)value;
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;

            // Get the index of a ListViewItem

            int index = listView.ItemContainerGenerator.IndexFromContainer(item);
            if (index % 2 == 0)
            {
                return Brushes.GhostWhite;
            }

            else
            {
                return Brushes.White;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
