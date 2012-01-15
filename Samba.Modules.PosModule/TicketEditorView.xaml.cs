using System.Windows.Controls;
using System.ComponentModel.Composition;

namespace Samba.Modules.PosModule
{
    /// <summary>
    /// Interaction logic for TicketEditorView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class TicketEditorView : UserControl
    {
        [ImportingConstructor]
        public TicketEditorView(TicketEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void UserControl_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = ((TicketEditorViewModel)DataContext).HandleTextInput(e.Text);
        }
    }
}
