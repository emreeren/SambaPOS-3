using System.ComponentModel;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    public class TaskEditorWidgetSettings
    {
        private NameWithValue _taskTypeNameValue;
        public NameWithValue TaskTypeNameValue
        {
            get { return _taskTypeNameValue ?? (_taskTypeNameValue = new NameWithValue()); }
        }

        [Browsable(false)]
        public string TaskTypeName { get { return TaskTypeNameValue.Text; } set { TaskTypeNameValue.Text = value; } }
        public string Caption { get; set; }
    }
}
