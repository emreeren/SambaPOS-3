using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Samba.Modules.PosModule
{
    /// <summary>
    /// Interaction logic for TicketListView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class TicketListView : UserControl
    {
        [ImportingConstructor]
        public TicketListView(TicketListViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
