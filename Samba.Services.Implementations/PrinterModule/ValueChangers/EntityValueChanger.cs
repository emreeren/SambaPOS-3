using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class EntityValueChanger : AbstractValueChanger<TicketEntity>
    {
        private static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();

        public override string GetTargetTag()
        {
            return "ENTITIES";
        }

        protected override string GetModelName(TicketEntity model)
        {
            var entityType = CacheService.GetEntityTypeById(model.EntityTypeId);
            return entityType == null ? "" : entityType.EntityName;
        }
    }
}
