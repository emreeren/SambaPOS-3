using System.Collections.Generic;
using Samba.Domain.Models.Entities;

namespace Samba.Presentation.Services
{
    public interface IEntityService
    {
        IEnumerable<EntityScreenItem> GetCurrentEntityScreenItems(EntityScreen entityScreen, int currentPageNo, string entityStateFilter);
        IEnumerable<Entity> GetEntitiesByState(string state, int entityTypeId);
        IList<EntityScreenItem> LoadEntityScreenItems(string selectedEntityScreen);
        IList<Widget> LoadWidgets(string selectedEntityScreen);
        void SaveEntityScreenItems();
        void UpdateEntityState(int entityId, int entityType, string stateName, string state);
        void AddWidgetToEntityScreen(string entityScreenName, Widget widget);
        void UpdateEntityScreen(EntityScreen entityScreen);
        void RemoveWidget(Widget widget);
        List<Entity> SearchEntities(string searchString, EntityType selectedEntityType, string stateFilter);
    }
}
