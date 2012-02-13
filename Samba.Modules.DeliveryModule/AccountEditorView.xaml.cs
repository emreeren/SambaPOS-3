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
    /// Interaction logic for AccountEditorView.xaml
    /// </summary>
    
    [Export]
    public partial class AccountEditorView : UserControl
    {
        [ImportingConstructor]
        public AccountEditorView(AccountEditorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AccountNameEdit.BackgroundFocus();
        }
    }
}
