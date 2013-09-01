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
using Samba.Domain.Models.Tickets;

namespace Samba.Modules.ModifierModule
{
    /// <summary>
    /// Interaction logic for AutomationCommandSelectorView.xaml
    /// </summary>

    [Export]
    public partial class AutomationCommandSelectorView : UserControl
    {
        [ImportingConstructor]
        public AutomationCommandSelectorView(AutomationCommandSelectorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        public AutomationCommandSelectorView()
        {
            InitializeComponent();
        }
    }
}
