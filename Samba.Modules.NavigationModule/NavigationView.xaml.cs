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

namespace Samba.Modules.NavigationModule
{
    /// <summary>
    /// Interaction logic for NavigationView.xaml
    /// </summary>

    [Export]
    public partial class NavigationView : UserControl
    {
        [ImportingConstructor]
        public NavigationView(NavigationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
