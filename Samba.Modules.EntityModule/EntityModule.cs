using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.EntityModule
{
    [ModuleExport(typeof(EntityModule))]
    class EntityModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly ICacheService _cacheService;
        private readonly IAccountService _accountService;
        private readonly EntityEditorView _entityEditorView;
        private readonly EntitySwitcherView _entitySwitcherView;
        private readonly IEntityService _entityService;
        private readonly IEntityServiceClient _entityServiceClient;

        [ImportingConstructor]
        public EntityModule(IRegionManager regionManager,
            IUserService userService, IApplicationStateSetter applicationStateSetter, ICacheService cacheService, IAccountService accountService,
            EntitySwitcherView entitySwitcherView, IAutomationService automationService, IEntityService entityService, IEntityServiceClient entityServiceClient,
            EntityEditorView entityEditorView)
        {
            _entitySwitcherView = entitySwitcherView;
            _entityService = entityService;
            _entityServiceClient = entityServiceClient;
            _entityEditorView = entityEditorView;
            _regionManager = regionManager;
            _applicationStateSetter = applicationStateSetter;
            _cacheService = cacheService;
            _accountService = accountService;

            AddDashboardCommand<EntityCollectionViewModelBase<EntityTypeViewModel, EntityType>>(Resources.EntityType.ToPlural(), Resources.Entities, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<EntityViewModel, Entity>>(Resources.Entity.ToPlural(), Resources.Entities, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<EntityScreenViewModel, EntityScreen>>(Resources.EntityScreen.ToPlural(), Resources.Entities, 41);

            automationService.RegisterActionType(ActionNames.CreateEntity, string.Format(Resources.Create_f, Resources.Entity), new { EntityTypeName = "", EntityName = "", CreateAccount = false });
            automationService.RegisterActionType(ActionNames.UpdateEntityState, Resources.UpdateEntityState, new { EntityTypeName = "", EntityStateName = "", CurrentState = "", EntityState = "", QuantityExp = "" });
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(EntitySwitcherView));
            _regionManager.RegisterViewWithRegion(RegionNames.EntityScreenRegion, typeof(EntitySelectorView));
            _regionManager.RegisterViewWithRegion(RegionNames.EntityScreenRegion, typeof(EntitySearchView));
            _regionManager.RegisterViewWithRegion(RegionNames.EntityScreenRegion, typeof(EntityEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.EntityScreenRegion, typeof(EntityDashboardView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Entity>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectEntity) ActivateEntitySwitcher();
                if (x.Topic == EventTopicNames.EditEntityDetails) ActivateEntityEditor();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<AccountData>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectEntity) ActivateEntitySwitcher();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<ActionData>>().Subscribe(OnActionData);
        }

        private void OnActionData(EventParameters<ActionData> actionData)
        {
            if (actionData.Value.Action.ActionType == ActionNames.CreateEntity)
            {
                var entityTypeName = actionData.Value.GetAsString("EntityTypeName");
                var entityName = actionData.Value.GetAsString("EntityName");
                var createAccount = actionData.Value.GetAsBoolean("CreateAccount");
                var customData = actionData.Value.GetAsString("CustomData");
                if (!string.IsNullOrEmpty(entityTypeName) && !string.IsNullOrEmpty(entityName))
                {
                    var entityType = _cacheService.GetEntityTypeByName(entityTypeName);
                    var entity = _entityService.CreateEntity(entityType.Id, entityName);
                    if (customData.Contains(":"))
                    {
                        foreach (var parts in customData.Split('#').Select(data => data.Split('=')))
                            entity.SetCustomData(parts[0], parts[1]);
                    }
                    if (createAccount)
                    {
                        var accountName = entityType.GenerateAccountName(entity);
                        var accountId = _accountService.CreateAccount(entityType.AccountTypeId, accountName);
                        entity.AccountId = accountId;
                        actionData.Value.DataObject.AccountName = accountName;
                    }
                    _entityService.SaveEntity(entity);
                    actionData.Value.DataObject.EntityName = entity.Name;
                }
            }

            if (actionData.Value.Action.ActionType == ActionNames.UpdateEntityState)
            {
                var entityId = actionData.Value.GetDataValueAsInt("EntityId");
                var entityTypeId = actionData.Value.GetDataValueAsInt("EntityTypeId");
                var stateName = actionData.Value.GetAsString("EntityStateName");
                var state = actionData.Value.GetAsString("EntityState");
                var quantityExp = actionData.Value.GetAsString("QuantityExp");
                if (state != null)
                {
                    if (entityId > 0 && entityTypeId > 0)
                    {
                        _entityServiceClient.UpdateEntityState(entityId, entityTypeId, stateName, state, quantityExp);
                    }
                    else
                    {
                        var ticket = actionData.Value.GetDataValue<Ticket>("Ticket");
                        if (ticket != null)
                        {
                            var entityTypeName = actionData.Value.GetAsString("EntityTypeName");
                            foreach (var ticketEntity in ticket.TicketEntities)
                            {
                                var entityType = _cacheService.GetEntityTypeById(ticketEntity.EntityTypeId);
                                if (string.IsNullOrEmpty(entityTypeName.Trim()) || entityType.Name == entityTypeName)
                                    _entityServiceClient.UpdateEntityState(ticketEntity.EntityId, ticketEntity.EntityTypeId, stateName, state, quantityExp);
                            }
                        }
                    }
                }
            }
        }

        private void ActivateEntityEditor()
        {
            _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.EntityView);
            _regionManager.ActivateRegion(RegionNames.EntityScreenRegion, _entityEditorView);
        }

        private void ActivateEntitySwitcher()
        {
            _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.EntityView);
            _regionManager.ActivateRegion(RegionNames.MainRegion, _entitySwitcherView);
        }
    }
}
