using System;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Tasks;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TaskModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class TaskTypeViewModel : EntityViewModelBase<TaskType>
    {
        [ImportingConstructor]
        public TaskTypeViewModel()
        {
       }


        public override Type GetViewType()
        {
            return typeof(TaskTypeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.TaskType;
        }
    }
}
