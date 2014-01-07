using System.ComponentModel.Composition;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.EntityModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class UdpateEntityData : ActionType
    {
        private readonly IEntityServiceClient _entityServiceClient;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public UdpateEntityData(IEntityServiceClient entityServiceClient, ICacheService cacheService)
        {
            _entityServiceClient = entityServiceClient;
            _cacheService = cacheService;
        }

        public override void Process(ActionData actionData)
        {
            var entityId = actionData.GetDataValueAsInt("EntityId");
            var entityName = actionData.GetAsString("EntityName");
            var fieldName = actionData.GetAsString("FieldName");
            var value = actionData.GetAsString("FieldValue");
            if (entityId > 0)
            {
                _entityServiceClient.UpdateEntityData(entityId, fieldName, value);
            }
            else if (!string.IsNullOrEmpty(entityName))
            {
                var entityTypeName = actionData.GetAsString("EntityTypeName");
                var entityType = _cacheService.GetEntityTypeByName(entityTypeName);
                if (entityType != null)
                {
                    _entityServiceClient.UpdateEntityData(entityType, entityName, fieldName, value);
                }
            }
            else
            {
                var ticket = actionData.GetDataValue<Ticket>("Ticket");
                if (ticket != null)
                {
                    var entityTypeName = actionData.GetAsString("EntityTypeName");
                    foreach (var ticketEntity in ticket.TicketEntities)
                    {
                        var entityType = _cacheService.GetEntityTypeById(ticketEntity.EntityTypeId);
                        if (string.IsNullOrEmpty(entityTypeName.Trim()) || entityType.Name == entityTypeName)
                            _entityServiceClient.UpdateEntityData(ticketEntity.EntityId, fieldName, value);
                    }
                }
            }

            var entity = actionData.GetDataValue<Entity>("Entity");
            if (entity != null && entity.Id == entityId)
                entity.SetCustomData(fieldName, value);
        }

        protected override object GetDefaultData()
        {
            return new { EntityTypeName = "", EntityName = "", FieldName = "", FieldValue = "" };
        }

        protected override string GetActionName()
        {
            return Resources.UpdateEntityData;
        }

        protected override string GetActionKey()
        {
            return "UpdateEntityData";
        }
    }
}
