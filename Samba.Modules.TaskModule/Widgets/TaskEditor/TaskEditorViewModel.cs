using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tasks;
using Samba.Infrastructure;
using Samba.Infrastructure.Helpers;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    public class TaskEditorViewModel : WidgetViewModel
    {
        private readonly ITaskService _taskService;
        private readonly ICacheService _cacheService;

        public TaskEditorViewModel(Widget widget, IApplicationState applicationState, ITaskService taskService, ICacheService cacheService)
            : base(widget, applicationState)
        {
            _taskService = taskService;
            _cacheService = cacheService;
            Tasks = new ObservableCollection<TaskViewModel>();
            AddTaskCommand = new CaptionCommand<string>(Resources.Add, OnAddTask);
        }

        [Browsable(false)]
        public ICaptionCommand AddTaskCommand { get; set; }

        [Browsable(false)]
        public TaskEditorWidgetSettings Settings { get { return SettingsObject as TaskEditorWidgetSettings; } }

        private int _taskTypeId;
        [Browsable(false)]
        public int TaskTypeId
        {
            get { return _taskTypeId > 0 ? _taskTypeId : (_taskTypeId = _cacheService.GetTaskTypeIdByName(Settings.TaskTypeName)); }
        }

        private string _newTask;
        [Browsable(false)]
        public string NewTask
        {
            get { return _newTask; }
            set { _newTask = value; RaisePropertyChanged(() => NewTask); }
        }

        [Browsable(false)]
        public ObservableCollection<TaskViewModel> Tasks { get; set; }

        private void OnAddTask(string obj)
        {
            if (!string.IsNullOrEmpty(NewTask))
            {
                if (TaskTypeId == 0)
                {
                    NewTask = "Can't add a task. Update Task Type from widget settings";
                    return;
                }
                var task = _taskService.AddNewTask(TaskTypeId, NewTask);
                var wm = new TaskViewModel(task, this);
                wm.Persist();
                NewTask = "";
            }
        }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<TaskEditorWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            Settings.TaskTypeNameValue.UpdateValues(_cacheService.GetTaskTypeNames());
        }

        public override void Refresh()
        {
            IEnumerable<Task> tasks = new List<Task>();

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                {
                    tasks = _taskService.SaveTasks(TaskTypeId, Tasks.Select(x => x.Model), AutoRefreshInterval);
                };

                worker.RunWorkerCompleted +=
                    delegate
                    {
                        Tasks.Clear();
                        Tasks.AddRange(tasks.OrderByDescending(x => x.Id).Select(x => new TaskViewModel(x, this)));
                    };

                worker.RunWorkerAsync();
            }
        }
    }
}
