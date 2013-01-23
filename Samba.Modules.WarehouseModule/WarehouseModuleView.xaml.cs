using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Samba.Modules.WarehouseModule
{
    /// <summary>
    /// Interaction logic for WarehouseModuleView.xaml
    /// </summary>
   
    [Export]
    public partial class WarehouseModuleView : UserControl
    {
        [ImportingConstructor]
        public WarehouseModuleView(WarehouseModuleViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
