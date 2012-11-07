using Samba.Domain.Models.Tasks;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    public class TaskViewModel : ObservableObject
    {
        public ICaptionCommand ToggleCompletedCommand { get; set; }

        public Task Model { get; set; }
        public TaskViewModel(Task model)
        {
            Model = model;
            ToggleCompletedCommand = new CaptionCommand<string>("£", OnToggleCompleted);
        }

        private void OnToggleCompleted(string obj)
        {
            IsCompleted = !IsCompleted;

        }

        public string IsCompletedCaption { get { return IsCompleted ? "R" : "£"; } }
        public bool IsCompleted
        {
            get { return Model.Completed; }
            set
            {
                Model.Completed = value;
                RaisePropertyChanged(() => IsCompleted);
                RaisePropertyChanged(() => IsCompletedCaption);
            }
        }
        public string Content { get { return Model.Content; } set { Model.Content = value; } }
    }
}