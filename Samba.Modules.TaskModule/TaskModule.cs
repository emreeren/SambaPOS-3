using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Tasks;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.TaskModule
{
    [ModuleExport(typeof(TaskModule))]
    public class TaskModule : ModuleBase
    {
        [ImportingConstructor]
        public TaskModule()
        {
            AddDashboardCommand<EntityCollectionViewModelBase<TaskTypeViewModel, TaskType>>(Resources.TaskType.ToPlural(), Resources.Settings, 20);
        }
    }
}
