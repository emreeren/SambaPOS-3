using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tasks;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.TaskModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class TaskTypeViewModel : EntityViewModelBase<TaskType>
    {
        [ImportingConstructor]
        public TaskTypeViewModel()
        {
            AddEntityTypeCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.EntityType), OnAddEntityType);
            DeleteEntityTypeCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.EntityType), OnDeleteEntityType, CanDeleteEntityType);
        }

        public ICaptionCommand AddEntityTypeCommand { get; set; }
        public ICaptionCommand DeleteEntityTypeCommand { get; set; }
        public EntityType SelectedEntityType { get; set; }

        private ObservableCollection<EntityType> _entityTypes;
        public ObservableCollection<EntityType> EntityTypes
        {
            get { return _entityTypes ?? (_entityTypes = new ObservableCollection<EntityType>(Model.EntityTypes)); }
        }

        private bool CanDeleteEntityType(string arg)
        {
            return SelectedEntityType != null;
        }

        private void OnDeleteEntityType(string obj)
        {
            Model.EntityTypes.Remove(SelectedEntityType);
            EntityTypes.Remove(SelectedEntityType);
        }

        private void OnAddEntityType(string obj)
        {
            var selectedValues =
                 InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<EntityType>().ToList<IOrderable>(),
                 Model.EntityTypes.ToList<IOrderable>(), Resources.EntityType.ToPlural(),
                 string.Format(Resources.SelectItemsFor_f, Resources.EntityType.ToPlural(), Model.Name, Resources.TaskType),
                 Resources.EntityType, Resources.EntityType.ToPlural());

            foreach (EntityType selectedValue in selectedValues)
            {
                if (!Model.EntityTypes.Contains(selectedValue))
                    Model.EntityTypes.Add(selectedValue);
            }

            _entityTypes = null;
            RaisePropertyChanged(() => EntityTypes);
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
