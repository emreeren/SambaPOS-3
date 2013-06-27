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

namespace Samba.Modules.AccountModule
{
    /// <summary>
    /// Interaction logic for AccountSelectorView.xaml
    /// </summary>

    [Export]
    public partial class AccountSelectorView : UserControl
    {
        [ImportingConstructor]
        public AccountSelectorView(AccountSelectorViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.Refreshed += viewModel_Refreshed;
            InitializeComponent();
        }

        void viewModel_Refreshed(object sender, EventArgs e)
        {
            (MainListView.View as GridView).Columns[0].Width = 0;
            (MainListView.View as GridView).Columns[0].Width = Double.NaN;
        }
    }
}
