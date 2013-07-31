using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.EntityModule
{
    [Export]
    public class EntityEditorViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly ITicketServiceBase _ticketServiceBase;
        private readonly IApplicationState _applicationState;
        private readonly IEntityService _entityService;
        public ICaptionCommand SaveEntityCommand { get; set; }
        public ICaptionCommand SelectEntityCommand { get; set; }
        public ICaptionCommand CreateAccountCommand { get; set; }

        [ImportingConstructor]
        public EntityEditorViewModel(ICacheService cacheService, IAccountService accountService, IUserService userService,
            ITicketServiceBase ticketServiceBase, IApplicationState applicationState, IEntityService entityService)
        {
            _cacheService = cacheService;
            _accountService = accountService;
            _userService = userService;
            _ticketServiceBase = ticketServiceBase;
            _applicationState = applicationState;
            _entityService = entityService;
            SaveEntityCommand = new CaptionCommand<string>(Resources.Save, OnSaveEntity, CanSelectEntity);
            SelectEntityCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.Entity).Replace(" ", "\r"), OnSelectEntity, CanSelectEntity);
            CreateAccountCommand = new CaptionCommand<string>(string.Format(Resources.Create_f, Resources.Account).Replace(" ", "\r"), OnCreateAccount, CanCreateAccount);
            EventServiceFactory.EventService.GetEvent<GenericEvent<OperationRequest<Entity>>>().Subscribe(OnEditEntity);
        }

        public bool IsEntitySelectorVisible
        {
            get
            {
                if (_applicationState.SelectedEntityScreen != null)
                {
                    var ticketType = _cacheService.GetTicketTypeById(_applicationState.SelectedEntityScreen.TicketTypeId);
                    return ticketType.EntityTypeAssignments.Any(x => x.EntityTypeId == SelectedEntity.EntityType.Id);
                }
                return false;
            }
        }

        private bool CanCreateAccount(string arg)
        {
            if (CustomDataViewModel == null) return false;
            if (!_userService.IsUserPermittedFor(PermissionNames.CreateAccount)) return false;
            CustomDataViewModel.Update();
            return SelectedEntity != null && SelectedEntity.Model.AccountId == 0 && SelectedEntity.EntityType.AccountTypeId > 0 && !string.IsNullOrEmpty(SelectedEntity.EntityType.GenerateAccountName(SelectedEntity.Model));
        }

        private void OnCreateAccount(string obj)
        {
            if (SelectedEntity.Model.Id == 0) SaveSelectedEntity();
            var accountName = SelectedEntity.EntityType.GenerateAccountName(SelectedEntity.Model);
            var accountId = _accountService.CreateAccount(SelectedEntity.EntityType.AccountTypeId, accountName);
            SelectedEntity.Model.AccountId = accountId;
            SaveSelectedEntity();
            _ticketServiceBase.UpdateAccountOfOpenTickets(SelectedEntity.Model);
            CommonEventPublisher.PublishEntityOperation(SelectedEntity.Model, EventTopicNames.SelectEntity, EventTopicNames.EntitySelected);
        }

        private bool CanSelectEntity(string arg)
        {
            return SelectedEntity != null && !string.IsNullOrEmpty(SelectedEntity.Name);
        }

        private void OnSelectEntity(string obj)
        {
            SaveSelectedEntity();
            _operationRequest.Publish(SelectedEntity.Model);
        }

        private void OnSaveEntity(string obj)
        {
            SaveSelectedEntity();
            CommonEventPublisher.PublishEntityOperation(SelectedEntity.Model, EventTopicNames.SelectEntity, EventTopicNames.EntitySelected);
        }

        private void SaveSelectedEntity()
        {
            CustomDataViewModel.Update();
            _entityService.SaveEntity(SelectedEntity.Model);
        }

        private OperationRequest<Entity> _operationRequest;

        private void OnEditEntity(EventParameters<OperationRequest<Entity>> obj)
        {
            if (obj.Topic == EventTopicNames.EditEntityDetails)
            {
                _operationRequest = obj.Value;
                var entityType = _cacheService.GetEntityTypeById(obj.Value.SelectedItem.EntityTypeId);
                SelectedEntity = new EntitySearchResultViewModel(obj.Value.SelectedItem, entityType);
                CustomDataViewModel = new EntityCustomDataViewModel(obj.Value.SelectedItem, entityType);
                RaisePropertyChanged(() => CustomDataViewModel);
                RaisePropertyChanged(() => IsEntitySelectorVisible);
            }
        }

        public string SelectEntityCommandCaption { get { return string.Format(Resources.Select_f, SelectedEntityName()).Replace(" ", "\r"); } }

        private string SelectedEntityName()
        {
            return SelectedEntity != null ? SelectedEntity.EntityType.EntityName : Resources.Entity;
        }

        private EntitySearchResultViewModel _selectedEntity;
        public EntitySearchResultViewModel SelectedEntity
        {
            get { return _selectedEntity; }
            set
            {
                _selectedEntity = value;
                SelectedEntity.AccountCustomDataViewModel.UpdateNewEntityQueryFields();
                RaisePropertyChanged(() => SelectedEntity);
                RaisePropertyChanged(() => SelectEntityCommandCaption);
            }
        }

        public EntityCustomDataViewModel CustomDataViewModel { get; set; }

    }
}
