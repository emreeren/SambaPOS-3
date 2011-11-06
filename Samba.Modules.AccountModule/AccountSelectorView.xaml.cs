using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule
{
    /// <summary>
    /// Interaction logic for AccountSelectorView.xaml
    /// </summary>

    [Export]
    public partial class AccountSelectorView : UserControl
    {
        readonly DependencyPropertyDescriptor _selectedIndexChange = DependencyPropertyDescriptor.FromProperty(Selector.SelectedIndexProperty, typeof(TabControl));

        [ImportingConstructor]
        public AccountSelectorView(AccountSelectorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _selectedIndexChange.AddValueChanged(MainTabControl, MyTabControlSelectedIndexChanged);
        }

        private void MyTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (((TabControl)sender).SelectedIndex == 1)
                PhoneNumberTextBox.BackgroundFocus();
        }

        private void FlexButtonClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            ((AccountSelectorViewModel)DataContext).RefreshSelectedAccount();
            PhoneNumber.BackgroundFocus();
        }

        private void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (((AccountSelectorViewModel)DataContext).SelectAccountCommand.CanExecute(""))
                    ((AccountSelectorViewModel)DataContext).SelectAccountCommand.Execute("");
            }
        }

        private void PhoneNumberPreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }

        private void AccountNamePreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }

        private void AddressPreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }

        private void PhoneNumberTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }

        private void TicketNoPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                e.Handled = true;
                if (((AccountSelectorViewModel)DataContext).FindTicketCommand.CanExecute(""))
                    ((AccountSelectorViewModel)DataContext).FindTicketCommand.Execute("");
            }
        }

        private void PhoneNumberLoaded(object sender, RoutedEventArgs e)
        {
            PhoneNumber.BackgroundFocus();
        }
    }
}
