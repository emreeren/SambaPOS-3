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
    /// Interaction logic for TicketNoteEditorView.xaml
    /// </summary>
    
    [Export]
    public partial class TicketNoteEditorView : UserControl
    {
        [ImportingConstructor]
        public TicketNoteEditorView(TicketNoteEditorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void TicketNoteEditorView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TicketNote.BackgroundFocus();
            TicketNote.CaretIndex = TicketNote.Text.Length;
        }
    }
}
