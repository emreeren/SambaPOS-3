using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Samba.Presentation.Common;

namespace Samba.Modules.DeliveryModule
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
