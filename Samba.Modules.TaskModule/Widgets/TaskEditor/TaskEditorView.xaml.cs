using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
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
            if(c!=null) c.BackgroundFocus();
        }

        private void TaskEditorView_OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusFirstEditor();
        }
    }
}
