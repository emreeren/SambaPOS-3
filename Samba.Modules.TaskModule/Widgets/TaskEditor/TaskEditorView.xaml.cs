using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Samba.Presentation.Common;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    /// <summary>
    /// Interaction logic for TaskEditorView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class TaskEditorView : UserControl
    {
        public TaskEditorView()
        {
            InitializeComponent();
        }

        public void ViewModel_TaskAdded(object sender, EventArgs e)
        {
            FocusFirstEditor();
        }

        private void FocusFirstEditor()
        {
            var c = ExtensionServices.GetVisualChild<TextBox>(CustomEditors);
            if (c != null) c.BackgroundFocus();
            else TextBox.BackgroundFocus();
        }

        private void TaskEditorView_OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusFirstEditor();
        }

        private void TaskEditorView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is TaskEditorViewModel && (DataContext as TaskEditorViewModel).AddTaskCommand.CanExecute(""))
            {
                (DataContext as TaskEditorViewModel).AddTaskCommand.Execute("");
            }
        }
    }
}
