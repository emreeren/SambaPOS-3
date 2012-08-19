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
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly ITicketService _ticketService;
        public ICaptionCommand SaveResourceCommand { get; set; }
        public ICaptionCommand SelectResourceCommand { get; set; }
        public ICaptionCommand CreateAccountCommand { get; set; }

        [ImportingConstructor]
        public ResourceEditorViewModel(ICacheService cacheService, IAccountService accountService, IUserService userService, ITicketService ticketService)
        {
            _cacheService = cacheService;
            _accountService = accountService;
            _userService = userService;
            _ticketService = ticketService;
            SaveResourceCommand = new CaptionCommand<string>(Resources.Save, OnSaveResource, CanSelectResource);
            SelectResourceCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.Resource).Replace(" ", "\r"), OnSelectResource, CanSelectResource);
            CreateAccountCommand = new CaptionCommand<string>(string.Format(Resources.Create_f, Resources.Account).Replace(" ", "\r"), OnCreateAccount, CanCreateAccount);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(OnEditResource);
        }

        private bool CanCreateAccount(string arg)
        {
            if (CustomDataViewModel == null) return false;
            if (!_userService.IsUserPermittedFor(PermissionNames.CreateAccount)) return false;
            CustomDataViewModel.Update();
            return SelectedResource != null && SelectedResource.Model.AccountId == 0 && SelectedResource.ResourceTemplate.AccountTemplateId > 0 && !string.IsNullOrEmpty(SelectedResource.ResourceTemplate.GetAccountName(SelectedResource.Model));
        }

        private void OnCreateAccount(string obj)
        {
            if (SelectedResource.Model.Id == 0) SaveSelectedResource();
            var accountName = SelectedResource.ResourceTemplate.GetAccountName(SelectedResource.Model);
            var accountId = _accountService.CreateAccount(accountName, SelectedResource.ResourceTemplate.AccountTemplateId);
            SelectedResource.Model.AccountId = accountId;
            SaveSelectedResource();
            _ticketService.UpdateAccountOfOpenTickets(SelectedResource.Model);
            CommonEventPublisher.PublishEntityOperation(SelectedResource.Model, EventTopicNames.SelectResource, EventTopicNames.ResourceSelected);
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
                RaisePropertyChanged(() => SelectResourceCommandCaption);
            }
        }

        public ResourceCustomDataViewModel CustomDataViewModel { get; set; }

    }
}
