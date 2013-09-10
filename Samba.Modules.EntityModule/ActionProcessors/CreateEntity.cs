using System.ComponentModel.Composition;
using System.Linq;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.EntityModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class CreateEntity : ActionType
    {
        private readonly ICacheService _cacheService;
        private readonly IEntityService _entityService;
        private readonly IAccountService _accountService;

        [ImportingConstructor]
        public CreateEntity(ICacheService cacheService, IEntityService entityService, IAccountService accountService)
        {
            _cacheService = cacheService;
            _entityService = entityService;
            _accountService = accountService;
        }

        protected override object GetDefaultData()
        {
            return new { EntityTypeName = "", EntityName = "", CustomData = "", CreateAccount = false };
        }

        protected override string GetActionName()
        {
            return string.Format(Resources.Create_f, Resources.Entity);
        }

        protected override string GetActionKey()
        {
            return ActionNames.CreateEntity;
        }

        public override void Process(ActionData actionData)
        {
            var entityTypeName = actionData.GetAsString("EntityTypeName");
            var entityName = actionData.GetAsString("EntityName");
            var createAccount = actionData.GetAsBoolean("CreateAccount");
            var customData = actionData.GetAsString("CustomData");
            if (!string.IsNullOrEmpty(entityTypeName) && !string.IsNullOrEmpty(entityName))
            {
                var entityType = _cacheService.GetEntityTypeByName(entityTypeName);
                var entity = _entityService.CreateEntity(entityType.Id, entityName);
                if (customData.Contains("="))
                {
                    foreach (var parts in customData.Split(';').Select(data => data.Split('=')))
                        entity.SetCustomData(parts[0], parts[1]);
                }
                if (createAccount)
                {
                    var accountName = entityType.GenerateAccountName(entity);
                    var accountId = _accountService.CreateAccount(entityType.AccountTypeId, accountName);
                    entity.AccountId = accountId;
                    actionData.DataObject.AccountName = accountName;
                }
                _entityService.SaveEntity(entity);
                actionData.DataObject.Entity = entity;
                actionData.DataObject.EntityName = entity.Name;
                actionData.DataObject.EntityId = entity.Id;
            }
        }
    }
}
