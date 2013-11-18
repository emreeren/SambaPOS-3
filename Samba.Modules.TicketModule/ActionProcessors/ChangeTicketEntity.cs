using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class ChangeTicketEntity : ActionType
    {
        private readonly ITicketService _ticketService;
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private readonly IEntityService _entityService;

        [ImportingConstructor]
        public ChangeTicketEntity(ITicketService ticketService, IApplicationState applicationState, ICacheService cacheService, IEntityService entityService)
        {
            _ticketService = ticketService;
            _applicationState = applicationState;
            _cacheService = cacheService;
            _entityService = entityService;
        }

        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");
            if ((ticket == null || ticket == Ticket.Empty) && actionData.GetAsBoolean("CanCreateTicket") && !_applicationState.IsLocked)
            {
                ticket = _ticketService.OpenTicket(0);
                actionData.DataObject.Ticket = ticket;
                ticket.PublishEvent(EventTopicNames.SetSelectedTicket);
            }

            if (ticket != null)
            {
                var entityTypeName = actionData.GetAsString("EntityTypeName");
                var entityName = actionData.GetAsString("EntityName");
                var customDataSearchValue = actionData.GetAsString("EntitySearchValue");
                var entityType = _cacheService.GetEntityTypeByName(entityTypeName);
                if (entityType != null)
                {
                    if (string.IsNullOrEmpty(entityName) && string.IsNullOrEmpty(customDataSearchValue))
                    {
                        CommonEventPublisher.PublishEntityOperation(Entity.GetNullEntity(entityType.Id), EventTopicNames.SelectEntity, EventTopicNames.EntitySelected);
                        return;
                    }

                    Entity entity = null;
                    if (!string.IsNullOrEmpty(customDataSearchValue))
                    {

                        var entities = _entityService.SearchEntities(entityType,
                                                                    customDataSearchValue,
                                                                    null);
                        if (entities.Count == 1)
                        {
                            entity = entities.First();
                        }
                    }

                    if (entity == null)
                    {
                        entity = _cacheService.GetEntityByName(entityTypeName, entityName);
                    }

                    if (entity == null && string.IsNullOrEmpty(entityName) && string.IsNullOrEmpty(customDataSearchValue))
                    {
                        entity = Entity.GetNullEntity(entityType.Id);
                    }

                    if (entity != null)
                    {
                        _ticketService.UpdateEntity(ticket, entity);
                        actionData.DataObject.EntityName = entity.Name;
                        actionData.DataObject.EntityId = entity.Id;
                        actionData.DataObject.CustomData = entity.CustomData;
                    }
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new { CanCreateTicket = false, EntityTypeName = "", EntityName = "", EntitySearchValue = "" };
        }

        protected override string GetActionName()
        {
            return Resources.ChangeTicketEntity;
        }

        protected override string GetActionKey()
        {
            return ActionNames.ChangeTicketEntity;
        }
    }
}
