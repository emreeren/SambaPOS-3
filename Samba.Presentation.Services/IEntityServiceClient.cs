using System.Collections.Generic;
using Samba.Domain.Models.Entities;

namespace Samba.Presentation.Services
{
    public interface IEntityServiceClient
    {
        void UpdateEntityState(int entityId, int entityType, string stateName, string state, string quantityExp);
        void UpdateEntityData(int entityId, string fieldName, string value);
    }
}
