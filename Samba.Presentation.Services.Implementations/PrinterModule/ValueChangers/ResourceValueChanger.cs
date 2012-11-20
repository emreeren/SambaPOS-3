using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Tickets;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.PrinterModule.ValueChangers
{
    public class ResourceValueChanger : AbstractValueChanger<TicketResource>
    {
        private static readonly IPresentationCacheService CacheService = ServiceLocator.Current.GetInstance<IPresentationCacheService>();

        public override string GetTargetTag()
        {
            return "RESOURCES";
        }

        protected override string GetModelName(TicketResource model)
        {
            var resourceType = CacheService.GetResourceTypeById(model.ResourceTypeId);
            return resourceType == null ? "" : resourceType.EntityName;
        }
    }
}
