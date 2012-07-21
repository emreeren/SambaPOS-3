using System.ComponentModel.Composition;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule
{
    [Export]
    public class ResourceEditorViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        public ICaptionCommand SaveResourceCommand { get; set; }
        public ICaptionCommand SelectResourceCommand { get; set; }

        [ImportingConstructor]
        public ResourceEditorViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            SaveResourceCommand = new CaptionCommand<string>(Resources.Save, OnSaveResource,CanSelectResource);
            SelectResourceCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.Resource).Replace(" ", "\r"), OnSelectResource, CanSelectResource);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(OnEditResource);
        }

        private bool CanSelectResource(string arg)
        {
            return SelectedResource != null && !string.IsNullOrEmpty(SelectedResource.Name);
        }

        private void OnSelectResource(string obj)
        {
            SaveSelectedResource();
            _operationRequest.Publish(SelectedResource.Model);
        }

        private void OnSaveResource(string obj)
        {
            SaveSelectedResource();
            CommonEventPublisher.PublishEntityOperation(SelectedResource.Model, EventTopicNames.SelectResource, EventTopicNames.ResourceSelected);
        }

        private void SaveSelectedResource()
        {
            CustomDataViewModel.Update();
            Dao.Save(SelectedResource.Model);
        }

        private EntityOperationRequest<Resource> _operationRequest;

        private void OnEditResource(EventParameters<EntityOperationRequest<Resource>> obj)
        {
            if (obj.Topic == EventTopicNames.EditResourceDetails)
            {
                _operationRequest = obj.Value;
                var resourceTemplate = _cacheService.GetResourceTemplateById(obj.Value.SelectedEntity.ResourceTemplateId);
                SelectedResource = new ResourceSearchResultViewModel(obj.Value.SelectedEntity, resourceTemplate);
                CustomDataViewModel = new ResourceCustomDataViewModel(obj.Value.SelectedEntity, resourceTemplate);
                SelectedResource.UpdateDetailedInfo();
                RaisePropertyChanged(() => CustomDataViewModel);
            }
        }

        public string SelectResourceCommandCaption { get { return string.Format(Resources.Select_f, SelectedEntityName()).Replace(" ", "\r"); } }

        private string SelectedEntityName()
        {
            return SelectedResource != null ? SelectedResource.ResourceTemplate.EntityName : Resources.Resource;
        }

        private ResourceSearchResultViewModel _selectedResource;
        public ResourceSearchResultViewModel SelectedResource
        {
            get { return _selectedResource; }
            set
            {
                _selectedResource = value;
                RaisePropertyChanged(() => SelectedResource);
                RaisePropertyChanged(()=>SelectResourceCommandCaption);
            }
        }

        public ResourceCustomDataViewModel CustomDataViewModel { get; set; }

    }
}
