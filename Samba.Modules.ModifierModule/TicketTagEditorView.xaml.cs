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
using Samba.Presentation.Common;

namespace Samba.Modules.ModifierModule
{
    /// <summary>
    /// Interaction logic for TicketTagEditorView.xaml
    /// </summary>
    
    [Export]
    public partial class TicketTagEditorView : UserControl
    {
        [ImportingConstructor]
        public TicketTagEditorView(TicketTagEditorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void GroupBox_IsVisibleChanged_2(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (((Control)sender).IsVisible)
                FreeTag.BackgroundFocus();
        }
    }
}
