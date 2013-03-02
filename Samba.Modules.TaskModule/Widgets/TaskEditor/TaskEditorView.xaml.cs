using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
    }
}
