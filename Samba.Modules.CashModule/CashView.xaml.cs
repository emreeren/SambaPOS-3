using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using Samba.Presentation.Common;

namespace Samba.Modules.CashModule
{
    /// <summary>
    /// Interaction logic for CashView.xaml
    /// </summary>
    
    [Export]
    public partial class CashView : UserControl
    {
        [ImportingConstructor]
        public CashView(CashViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void FilteredTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            DescriptionEditor.BackgroundFocus();
        }
    }
}
