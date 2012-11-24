using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
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
            AddResourceTypeCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.ResourceType), OnAddResourceType);
            DeleteResourceTypeCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.ResourceType), OnDeleteResourceType, CanDeleteResourceType);
        }

        public ICaptionCommand AddResourceTypeCommand { get; set; }
        public ICaptionCommand DeleteResourceTypeCommand { get; set; }
        public ResourceType SelectedResourceType { get; set; }

        private ObservableCollection<ResourceType> _resourceTypes;
        public ObservableCollection<ResourceType> ResourceTypes
        {
            get { return _resourceTypes ?? (_resourceTypes = new ObservableCollection<ResourceType>(Model.ResourceTypes)); }
        }

        private bool CanDeleteResourceType(string arg)
        {
            return SelectedResourceType != null;
        }

        private void OnDeleteResourceType(string obj)
        {
            Model.ResourceTypes.Remove(SelectedResourceType);
            ResourceTypes.Remove(SelectedResourceType);
        }

        private void OnAddResourceType(string obj)
        {
            var selectedValues =
                 InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<ResourceType>().ToList<IOrderable>(),
                 Model.ResourceTypes.ToList<IOrderable>(), Resources.ResourceType.ToPlural(),
                 string.Format(Resources.SelectItemsFor_f, Resources.ResourceType.ToPlural(), Model.Name, Resources.TaskType),
                 Resources.ResourceType, Resources.ResourceType.ToPlural());

            foreach (ResourceType selectedValue in selectedValues)
            {
                if (!Model.ResourceTypes.Contains(selectedValue))
                    Model.ResourceTypes.Add(selectedValue);
            }

            _resourceTypes = null;
            RaisePropertyChanged(() => ResourceTypes);
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
