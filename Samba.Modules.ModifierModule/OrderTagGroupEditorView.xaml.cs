using System.ComponentModel.Composition;
using System.Windows.Controls;
using Samba.Presentation.Common;

namespace Samba.Modules.ModifierModule
{
    /// <summary>
    /// Interaction logic for SelectedOrdersView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class OrderTagGroupEditorView : UserControl
    {
        [ImportingConstructor]
        public OrderTagGroupEditorView(OrderTagGroupEditorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
