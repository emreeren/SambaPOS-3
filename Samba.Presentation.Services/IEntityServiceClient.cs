using System.Collections.Generic;

namespace Samba.Presentation.Services
{
    public interface IEntityServiceClient
    {
        void UpdateEntityState(int entityId, int entityType, string stateName, string state);
    }
}
