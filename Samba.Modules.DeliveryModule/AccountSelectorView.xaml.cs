using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Samba.Presentation.Common;

namespace Samba.Modules.DeliveryModule
{
    /// <summary>
    /// Interaction logic for AccountSearchView.xaml
    /// </summary>
    
    [Export]
    public partial class AccountSelectorView : UserControl
    {
        [ImportingConstructor]
        public AccountSelectorView(AccountSelectorViewModel viewModel)
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

        private void FlexButtonClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            ((AccountSelectorViewModel)DataContext).RefreshSelectedAccount();
            SearchString.BackgroundFocus();
        }

        private void SearchStringLoaded(object sender, RoutedEventArgs e)
        {
            SearchString.BackgroundFocus();
        }
    }
}
