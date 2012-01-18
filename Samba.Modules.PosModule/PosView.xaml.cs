using System.Windows.Controls;
using System.ComponentModel.Composition;

namespace Samba.Modules.PosModule
{
    /// <summary>
    /// Interaction logic for TicketEditorView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class PosView : UserControl
    {
        [ImportingConstructor]
        public PosView(PosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void UserControl_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = ((PosViewModel)DataContext).HandleTextInput(e.Text);
        }
    }
}
