using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Samba.Modules.ResourceModule
{
    /// <summary>
    /// Interaction logic for LocationSelectorView.xaml
    /// </summary>

    [Export]
    public partial class ResourceSelectorView : UserControl
    {
        [ImportingConstructor]
        public ResourceSelectorView(ResourceSelectorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
