using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tasks;
using Samba.Infrastructure.Helpers;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    public class TaskEditorViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly ITaskService _taskService;
        private readonly ICacheService _cacheService;
        private readonly IMessagingService _messagingService;

        public event EventHandler TaskAdded;

        protected virtual void OnTaskAdded()
        {
            EventHandler handler = TaskAdded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public TaskEditorViewModel(Widget widget, IApplicationState applicationState, ITaskService taskService,
            ICacheService cacheService, IMessagingService messagingService)
            : base(widget, applicationState)
        {
            _applicationState = applicationState;
            _taskService = taskService;
            _cacheService = cacheService;
            _messagingService = messagingService;
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

        private TaskType _taskType;
        [Browsable(false)]
        public TaskType TaskType
        {
            get { return _taskType ?? (_taskType = _cacheService.GetTaskTypeByName(Settings.TaskTypeName)); }
        }

        private IEnumerable<TaskCustomFieldEditorModel> _customFields;
        [Browsable(false)]
        public IEnumerable<TaskCustomFieldEditorModel> CustomFields
        {
            get { return _customFields ?? (_customFields = TaskType != null ? TaskType.TaskCustomFields.Select(x => new TaskCustomFieldEditorModel(x)).ToList() : null); }
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
            if (!string.IsNullOrWhiteSpace(NewTask) || CustomFields.Any(x => !string.IsNullOrWhiteSpace(x.Value)))
            {
                if (TaskTypeId == 0)
                {
                    NewTask = "Can't add a task. Update Task Type from widget settings";
                    return;
                }
                var task = _taskService.AddNewTask(TaskTypeId, NewTask, CustomFields.ToDictionary(x => x.Name, x => x.Value), !Settings.DontCreateTaskHistory);
                foreach (var customField in CustomFields)
                {
                    customField.Value = "";
                }
                if (!Settings.DontCreateTaskHistory)
                {
                    var wm = new TaskViewModel(task, TaskType, this, _messagingService);
                    wm.Persist();
                }
                ExecuteTaskCreateCommands(task);
                NewTask = "";
                OnTaskAdded();
            }
        }

        public void ExecuteTaskCompletedCommands(Task task)
        {
            ExecuteCommands(Settings.TaskCompleteCommandList, task);
        }

        public void ExecuteTaskCreateCommands(Task task)
        {
            ExecuteCommands(Settings.TaskCreateCommandList, task);
        }

        public void ExecuteTaskCommand(Task task, string commandName)
        {
            var command = Settings.TaskCommandList.FirstOrDefault(x => x.DisplayName == commandName);
            if (command != null) ExecuteCommand(task, command, new ExpandoObject());
        }

        private void ExecuteCommands(IEnumerable<TaskCommand> commands, Task task)
        {
            var dataObject = new ExpandoObject();
            foreach (var command in commands)
            {
                ExecuteCommand(task, command, dataObject);
            }
        }

        private void ExecuteCommand(Task task, TaskCommand command, dynamic dataObject)
        {
            dataObject.AutomationCommandName = command.CommandName;
            dataObject.CommandValue = command.GetCommandValue(task);
            _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted, dataObject);
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
                        var sortedTasks = tasks.OrderBy(x => x.Completed).ThenByDescending(x => x.EndDate.Ticks);
                        Tasks.AddRange(sortedTasks.Select(x => new TaskViewModel(x, TaskType, this, _messagingService)));
                    };

                worker.RunWorkerAsync();
            }
        }

        public IEnumerable<string> GetTaskCommandNames()
        {
            return Settings.TaskCommandList.Select(x => x.DisplayName);
        }
    }
}
