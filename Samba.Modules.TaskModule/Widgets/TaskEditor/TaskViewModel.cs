using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tasks;
using Samba.Infrastructure.Messaging;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Services;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    public class TaskViewModel : ObservableObject
    {
        private readonly TaskType _taskType;
        private readonly TaskEditorViewModel _widget;
        private readonly IMessagingService _messagingService;

        public ICaptionCommand ToggleCompletedCommand { get; set; }
        public DelegateCommand<string> ExecuteCommand { get; set; }

        public Task Model { get; set; }
        public TaskViewModel(Task model, TaskType taskType, TaskEditorViewModel widget, IMessagingService messagingService)
        {
            _taskType = taskType;
            _widget = widget;
            _messagingService = messagingService;
            Model = model;
            ToggleCompletedCommand = new CaptionCommand<string>("¨", OnToggleCompleted);
            ExecuteCommand = new DelegateCommand<string>(OnCommandExecute);
        }

        public string IsCompletedCaption { get { return IsCompleted ? "þ" : "¨"; } }
        public bool IsCompleted
        {
            get { return Model.Completed; }
            set
            {
                Model.SetCompleted(value);
                Persist();
                RaisePropertyChanged(() => IsCompleted);
                RaisePropertyChanged(() => IsCompletedCaption);
            }
        }
        public string Content { get { return Model.Content ?? ""; } set { Model.Content = value; Persist(); } }

        public string CustomFieldValues
        {
            get
            {
                return string.Join(",", _taskType.TaskCustomFields.Where(x => !string.IsNullOrEmpty(Model.GetCustomDataValue(x.Name)))
                    .Select(x => x.GetFormattedValue(Model.GetCustomDataValue(x.Name))));
            }
        }
        public string Description { get { return Tokens != null ? string.Join(", ", Tokens.Select(x => x.Caption)) : ""; } }

        public bool IsDescriptionVisible { get { return !string.IsNullOrEmpty(Description); } }
        public bool IsCustomFieldValuesVisible { get { return !string.IsNullOrEmpty(CustomFieldValues); } }

        private IEnumerable<TaskTokenViewModel> _tokens;
        public IEnumerable<TaskTokenViewModel> Tokens
        {
            get { return _tokens ?? (_tokens = CreateTokens()); }
        }

        private IEnumerable<string> _taskCommands;
        public IEnumerable<string> TaskCommands { get { return _taskCommands ?? (_taskCommands = _widget.GetTaskCommandNames()); } }

        private IEnumerable<TaskTokenViewModel> CreateTokens()
        {
            return Content.Split(',').Select(CreateTaskTokenViewModel);
        }

        private TaskTokenViewModel CreateTaskTokenViewModel(string part)
        {
            return Model.TaskTokens.Any(x => x.Value == part)
                ? new TaskTokenViewModel(Model.TaskTokens.First(x => x.Value == part))
                : new TaskTokenViewModel(new TaskToken { Caption = part.Trim() });
        }

        private void OnCommandExecute(string obj)
        {
            _widget.ExecuteTaskCommand(Model, obj);
        }

        private void OnToggleCompleted(string obj)
        {
            IsCompleted = !IsCompleted;
            if (IsCompleted)
                _widget.ExecuteTaskCompletedCommands(Model);
            else _widget.ExecuteTaskCreateCommands(Model);
        }

        public void Persist()
        {
            Model.LastUpdateTime = DateTime.Now;
            _widget.Refresh();
            _messagingService.SendMessage(Messages.WidgetRefreshMessage, _widget.CreatorName);
        }
    }

    public class TaskTokenViewModel
    {
        private readonly TaskToken _model;
        public TaskTokenViewModel(TaskToken model)
        {
            _model = model;
        }

        public string Caption { get { return string.Format(TitleFormat, _model.Caption); } }
        private string TitleFormat
        {
            get
            {
                return "{0}";
            }
        }
    }
}