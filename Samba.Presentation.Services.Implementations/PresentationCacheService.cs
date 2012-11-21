using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Services.Implementations
{
    [Export(typeof(IPresentationCacheService))]
    class PresentationCacheService : AbstractService, IPresentationCacheService
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PresentationCacheService(IApplicationState applicationState, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
        }

        public override void Reset()
        {
            
        }
    }
}
