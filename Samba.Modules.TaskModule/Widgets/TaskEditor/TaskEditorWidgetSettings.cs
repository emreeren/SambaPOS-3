using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PropertyTools.DataAnnotations;
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

        [WideProperty]
        [Height(80)]
        public string TaskCreateCommands { get; set; }

        [WideProperty]
        [Height(80)]
        public string TaskCompleteCommands { get; set; }

        [WideProperty]
        [Height(80)]
        public string TaskCommands { get; set; }

        public bool DontCreateTaskHistory { get; set; }

        [Browsable(false)]
        internal IEnumerable<TaskCommand> TaskCreateCommandList { get { return GetTaskCommands(TaskCreateCommands); } }

        [Browsable(false)]
        internal IEnumerable<TaskCommand> TaskCompleteCommandList { get { return GetTaskCommands(TaskCompleteCommands); } }

        [Browsable(false)]
        internal IEnumerable<TaskCommand> TaskCommandList { get { return GetTaskCommands(TaskCommands); } }

        private static IEnumerable<TaskCommand> GetTaskCommands(string taskCreateCommands)
        {
            var lines = (taskCreateCommands??"").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x => new TaskCommand(x));
        }
    }
}
