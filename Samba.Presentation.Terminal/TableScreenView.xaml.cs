using System;
using System.Collections.Generic;
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

namespace Samba.Presentation.Terminal
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableScreenView : UserControl
    {
        public TableScreenView()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ts = (DataContext as TableScreenViewModel);
            if (ts != null)
            {
                if (ts.SelectedTableScreen.NumeratorHeight < 30)
                    ts.DisplayFullScreenNumerator();
            }

        }
    }
}
