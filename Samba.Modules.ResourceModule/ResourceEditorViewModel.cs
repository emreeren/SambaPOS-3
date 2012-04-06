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
        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand SaveResourceCommand { get; set; }
        public ICaptionCommand SelectResourceCommand { get; set; }

        [ImportingConstructor]
        public ResourceEditorViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreen);
            SaveResourceCommand = new CaptionCommand<string>(Resources.Save, OnSaveResource);
            SelectResourceCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.Resource).Replace(" ", "\r"), OnSelectResource);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(OnEditResource);
        }

        private void OnSelectResource(string obj)
        {
            SaveSelectedResource();
            _operationRequest.Publish(SelectedResource.Model);
        }

        private void OnSaveResource(string obj)
        {
            SaveSelectedResource();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateResourceView);
            //CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateAccountView);
        }

        private void SaveSelectedResource()
        {
            CustomDataViewModel.Update();
            Dao.Save(SelectedResource.Model);
            //using (var ws = WorkspaceFactory.Create())
            //{
            //    if (!SelectedAccount.IsNotNew)
            //    {
            //        ws.Add(SelectedAccount.Model);
            //        ws.CommitChanges();

            //    }
            //    else
            //    {
            //        var result = ws.Single<Account>(
            //            x => x.Id == SelectedAccount.Id
            //                && x.Name == SelectedAccount.Name
            //                && x.CustomData == SelectedAccount.Model.CustomData);

            //        if (result == null)
            //        {
            //            result = ws.Single<Account>(x => x.Id == SelectedAccount.Id);
            //            Debug.Assert(result != null);
            //            result.Name = SelectedAccount.Name;
            //            result.CustomData = SelectedAccount.Model.CustomData;
            //            ws.CommitChanges();
            //        }
            //    }
            //}
        }

        private static void OnCloseScreen(string obj)
        {
            CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateResourceView);
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
                RaisePropertyChanged(() => CustomDataViewModel);
            }
        }

        private ResourceSearchResultViewModel _selectedResource;
        public ResourceSearchResultViewModel SelectedResource
        {
            get { return _selectedResource; }
            set
            {
                _selectedResource = value;
                RaisePropertyChanged(() => SelectedResource);
            }
        }

        public ResourceCustomDataViewModel CustomDataViewModel { get; set; }

    }
}
