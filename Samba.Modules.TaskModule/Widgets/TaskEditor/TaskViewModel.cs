using Samba.Domain.Models.Tasks;
using Samba.Presentation.Common;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    public class TaskViewModel : ObservableObject
    {
        public Task Model { get; set; }
        public TaskViewModel(Task model)
        {
            Model = model;
        }

        public string Content { get { return Model.Content; } set { Model.Content = value; } }
    }
}