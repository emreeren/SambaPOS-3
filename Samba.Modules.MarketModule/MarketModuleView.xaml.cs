using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Samba.Modules.MarketModule
{
    /// <summary>
    /// Interaction logic for MenuModuleView.xaml
    /// </summary>
    
    [Export]
    public partial class MarketModuleView : UserControl
    {
        [ImportingConstructor]
        public MarketModuleView(MarketModuleViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
