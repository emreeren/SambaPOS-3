using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Samba.Modules.AccountModule
{
    /// <summary>
    /// Interaction logic for AccountSelectorView.xaml
    /// </summary>

    [Export]
    public partial class DeliveryView : UserControl
    {
        [ImportingConstructor]
        public DeliveryView(DeliveryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
