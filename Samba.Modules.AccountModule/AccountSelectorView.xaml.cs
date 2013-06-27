using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;

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
            var view = MainListView.View as GridView;
            if (view != null)
            {
                view.Columns[0].Width = 0;
                view.Columns[0].Width = Double.NaN;
                view.Columns[1].Width = 0;
                view.Columns[1].Width = Double.NaN;
            }
        }
    }
}
