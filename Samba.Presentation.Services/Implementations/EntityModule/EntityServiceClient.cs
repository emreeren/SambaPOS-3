using System.ComponentModel.Composition;
using Samba.Domain.Models.Entities;
using Samba.Persistance;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.EntityModule
{
    [Export(typeof(IEntityServiceClient))]
    public class EntityServiceClient : IEntityServiceClient
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private readonly IEntityDao _entityDao;

        [ImportingConstructor]
        public EntityServiceClient(IApplicationState applicationState, ICacheService cacheService, IEntityDao entityDao)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            _entityDao = entityDao;
        }

        public void UpdateEntityState(int entityId, int entityTypeId, string stateName, string state, string quantityExp)
        {
            var sv = _entityDao.UpdateEntityState(entityId, stateName, state, quantityExp);
            var rt = _cacheService.GetEntityTypeById(entityTypeId);
            _applicationState.NotifyEvent(RuleEventNames.EntityStateUpdated, new
            {
                EntityId = entityId,
                EntityTypeName = rt.Name,
                StateName = stateName,
                State = state,
                Quantity = sv.GetStateQuantity(stateName)
            });
        }

    }
}
