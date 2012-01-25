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

namespace Samba.Modules.DeliveryModule
{
    /// <summary>
    /// Interaction logic for AccountSearchView.xaml
    /// </summary>
    
    [Export]
    public partial class AccountSearcherView : UserControl
    {
        [ImportingConstructor]
        public AccountSearcherView(AccountSearcherViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void SearchStringPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (((AccountSelectorViewModel)DataContext).SelectAccountCommand.CanExecute(""))
                    ((AccountSelectorViewModel)DataContext).SelectAccountCommand.Execute("");
            }
        }

        private void TicketNoPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (((AccountSelectorViewModel)DataContext).FindTicketCommand.CanExecute(""))
                    ((AccountSelectorViewModel)DataContext).FindTicketCommand.Execute("");
            }
        }

        private void SearchStringLoaded(object sender, RoutedEventArgs e)
        {
            SearchString.BackgroundFocus();
        }
    }
}
