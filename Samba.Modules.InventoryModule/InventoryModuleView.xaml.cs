using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Samba.Modules.InventoryModule
{
    /// <summary>
    /// Interaction logic for InventoryModuleView.xaml
    /// </summary>

    [Export]
    public partial class InventoryModuleView : UserControl
    {
        [ImportingConstructor]
        public InventoryModuleView(InventoryModuleViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
