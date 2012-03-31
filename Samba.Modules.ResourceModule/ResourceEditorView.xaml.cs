using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Samba.Presentation.Common;

namespace Samba.Modules.ResourceModule
{
    /// <summary>
    /// Interaction logic for AccountEditorView.xaml
    /// </summary>
    
    [Export]
    public partial class ResourceEditorView : UserControl
    {
        [ImportingConstructor]
        public ResourceEditorView(AccountEditorViewModel viewModel)
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
