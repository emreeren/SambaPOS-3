using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.EntityModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class UpdateEntityState : ActionType
    {
        private readonly IEntityServiceClient _entityServiceClient;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public UpdateEntityState(IEntityServiceClient entityServiceClient, ICacheService cacheService)
        {
            _entityServiceClient = entityServiceClient;
            _cacheService = cacheService;
        }

        protected override object GetDefaultData()
        {
            return new { EntityTypeName = "", EntityStateName = "", CurrentState = "", EntityState = "", QuantityExp = "" };
        }

        protected override string GetActionName()
        {
            return Resources.UpdateEntityState;
        }

        protected override string GetActionKey()
        {
            return ActionNames.UpdateEntityState;
        }

        public override void Process(ActionData actionData)
        {
            var entityId = actionData.GetDataValueAsInt("EntityId");
            var entityTypeId = actionData.GetDataValueAsInt("EntityTypeId");
            var stateName = actionData.GetAsString("EntityStateName");
            var state = actionData.GetAsString("EntityState");
            var quantityExp = actionData.GetAsString("QuantityExp");
            if (state != null)
            {
                if (entityId > 0 && entityTypeId > 0)
                {
                    _entityServiceClient.UpdateEntityState(entityId, entityTypeId, stateName, state, quantityExp);
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
                                _entityServiceClient.UpdateEntityState(ticketEntity.EntityId, ticketEntity.EntityTypeId, stateName, state, quantityExp);
                        }
                    }
                }
            }
        }
    }
}
