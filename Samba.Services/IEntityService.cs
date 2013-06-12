using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Entities;

namespace Samba.Services
{
    public interface IEntityService
    {
        IEnumerable<EntityScreenItem> GetCurrentEntityScreenItems(EntityScreen entityScreen, int currentPageNo, string entityStateFilter);
        IEnumerable<Entity> GetEntitiesByState(string state, int entityTypeId);
        IList<EntityScreenItem> LoadEntityScreenItems(string selectedEntityScreen);
        IList<Widget> LoadWidgets(string selectedEntityScreen);
        void SaveEntityScreenItems();
        void AddWidgetToEntityScreen(string entityScreenName, Widget widget);
        void UpdateEntityScreen(EntityScreen entityScreen);
        void RemoveWidget(Widget widget);
        List<Entity> SearchEntities(EntityType selectedEntityType, string searchString, string stateFilter);
    }
}
