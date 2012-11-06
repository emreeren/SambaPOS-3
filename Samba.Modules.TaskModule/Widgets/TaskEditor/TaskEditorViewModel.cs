using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tasks;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Services;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    public class TaskEditorViewModel : WidgetViewModel
    {
        private readonly ITaskService _taskService;

        public TaskEditorViewModel(Widget widget, ITaskService taskService)
            : base(widget)
        {
            _taskService = taskService;
            Tasks = new ObservableCollection<TaskViewModel>();
            AddTaskCommand = new CaptionCommand<string>("Add Task", OnAddTask);
        }

        public ICaptionCommand AddTaskCommand { get; set; }

        private string _newTask;
        public string NewTask
        {
            get { return _newTask; }
            set { _newTask = value; RaisePropertyChanged(() => NewTask); }
        }

        public ObservableCollection<TaskViewModel> Tasks { get; set; }

        private void OnAddTask(string obj)
        {
            if (!string.IsNullOrEmpty(NewTask))
            {
                var task = _taskService.AddNewTask(NewTask);
                Tasks.Insert(0, new TaskViewModel(task));
                NewTask = "";
            }
        }

        protected override object CreateSettingsObject()
        {
            return null;
        }

        public override void Refresh()
        {
            Tasks.Clear();
            Tasks.AddRange(_taskService.GetTasks().OrderByDescending(x => x.Id).Select(x => new TaskViewModel(x)));
        }
    }
}
