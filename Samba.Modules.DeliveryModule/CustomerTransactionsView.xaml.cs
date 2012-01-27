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

namespace Samba.Modules.DeliveryModule
{
    /// <summary>
    /// Interaction logic for AccountTransactionsView.xaml
    /// </summary>
    
    [Export]
    public partial class CustomerTransactionsView : UserControl
    {
        [ImportingConstructor]
        public CustomerTransactionsView(CustomerTransactionsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
