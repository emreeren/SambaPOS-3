using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Samba.Modules.PosModule
{
    /// <summary>
    /// Interaction logic for OpenTicketView.xaml
    /// </summary>

    [Export]
    public partial class OpenTicketsView : UserControl
    {
        [ImportingConstructor]
        public OpenTicketsView(OpenTicketsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
