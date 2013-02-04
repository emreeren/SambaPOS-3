using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Samba.Presentation.Common;

namespace Samba.Modules.EntityModule
{
    /// <summary>
    /// Interaction logic for AccountEditorView.xaml
    /// </summary>
    
    [Export]
    public partial class EntityEditorView : UserControl
    {
        [ImportingConstructor]
        public EntityEditorView(EntityEditorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            EntityNameEdit.BackgroundFocus();
        }
    }
}
