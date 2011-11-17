using System;
using System.Collections.Generic;
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

namespace Samba.Presentation.Terminal
{
    /// <summary>
    /// Interaction logic for SelectedOrderEditorView.xaml
    /// </summary>
    public partial class SelectedOrderEditorView : UserControl
    {
        public SelectedOrderEditorView()
        {
            InitializeComponent();
        }

        private void ExtraPropertyName_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ExtraPropertyName.BackgroundSelectAll();
            (DataContext as SelectedOrderEditorViewModel).ShowKeyboard();
        }

        private void ExtraPropertyName_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            (DataContext as SelectedOrderEditorViewModel).HideKeyboard();
        }

        private void ExtraPropertyPrice_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ExtraPropertyPrice.BackgroundSelectAll();
            (DataContext as SelectedOrderEditorViewModel).ShowKeyboard();
        }

        private void ExtraPropertyPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            (DataContext as SelectedOrderEditorViewModel).HideKeyboard();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ExtraPropertyName.Focus();
        }

        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            ExtraPropertyPrice.Focus();
        }

        private void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (NoteEditor.Visibility == Visibility.Visible)
            {
                NoteEditor.BackgroundFocus();
                NoteEditor.BackgroundSelectAll();
            }
        }

        private void FreeTagEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            (DataContext as SelectedOrderEditorViewModel).IsKeyboardVisible = true;
        }
    }
}
