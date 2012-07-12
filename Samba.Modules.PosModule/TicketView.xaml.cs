using System.ComponentModel.Composition;
using System.Windows.Controls;
using Samba.Presentation.Common;

namespace Samba.Modules.PosModule
{
    /// <summary>
    /// Interaction logic for TicketView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class TicketView : UserControl
    {
        [ImportingConstructor]
        public TicketView(TicketViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
