using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Samba.Presentation.Common;

namespace Samba.Modules.DeliveryModule
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
            //if (((TabControl)sender).SelectedIndex == 1)
            //    PhoneNumberTextBox.BackgroundFocus();
        }

        private void FlexButtonClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            ((AccountSelectorViewModel)DataContext).RefreshSelectedAccount();
            //SearchString.BackgroundFocus();
        }
    }
}
