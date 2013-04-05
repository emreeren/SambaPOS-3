using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tasks;
using Samba.Infrastructure.Messaging;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    public class TaskViewModel : ObservableObject
    {
        private readonly IDiagram _widget;
        public ICaptionCommand ToggleCompletedCommand { get; set; }

        public Task Model { get; set; }
        public TaskViewModel(Task model, IDiagram widget)
        {
            _widget = widget;
            Model = model;
            ToggleCompletedCommand = new CaptionCommand<string>("¨", OnToggleCompleted);
        }

        private void OnToggleCompleted(string obj)
        {
            IsCompleted = !IsCompleted;
        }

        public void Persist()
        {
            Model.LastUpdateTime = DateTime.Now;
            _widget.Refresh();
            AppServices.MessagingService.SendMessage(Messages.WidgetRefreshMessage, _widget.CreatorName);
        }

        public string IsCompletedCaption { get { return IsCompleted ? "þ" : "¨"; } }
        public bool IsCompleted
        {
            get { return Model.Completed; }
            set
            {
                Model.Completed = value;
                Persist();
                RaisePropertyChanged(() => IsCompleted);
                RaisePropertyChanged(() => IsCompletedCaption);
            }
        }
        public string Content { get { return Model.Content; } set { Model.Content = value; Persist(); } }
        public string Description { get { return string.Join(", ", Tokens.Select(x => x.Caption)); } }

        private IEnumerable<TaskTokenViewModel> _tokens;
        public IEnumerable<TaskTokenViewModel> Tokens
        {
            get { return _tokens ?? (_tokens = CreateTokens()); }
        }

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