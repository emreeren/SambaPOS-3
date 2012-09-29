using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class ResourceValueChanger : AbstractValueChanger<TicketResource>
    {
        private static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();

        public override string GetTargetTag()
        {
            return "RESOURCES";
        }

        protected override string GetModelName(TicketResource model)
        {
            var ResourceType = CacheService.GetResourceTypeById(model.ResourceTypeId);
            return ResourceType == null ? "" : ResourceType.EntityName;
        }
    }
}
