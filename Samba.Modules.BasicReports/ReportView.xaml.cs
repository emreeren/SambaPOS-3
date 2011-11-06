using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Samba.Modules.BasicReports
{
    /// <summary>
    /// Interaction logic for ReportDisplay.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var browsable = ((PropertyDescriptor)e.PropertyDescriptor).Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
            if (browsable != null && !browsable.Browsable)
            {
                e.Cancel = true;
                return;
            }

            var displayName = ((PropertyDescriptor)e.PropertyDescriptor).Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
            if (displayName != null && !string.IsNullOrEmpty(displayName.DisplayName))
            {
                e.Column.Header = displayName.DisplayName;
            }
            else e.Column.Header = e.PropertyName;
        }
    }
}
