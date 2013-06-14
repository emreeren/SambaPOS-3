﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Data;
using Samba.Persistance;
using Samba.Persistance.Data;

namespace Samba.Services.Implementations.EntityModule
{
    [Export(typeof(IEntityService))]
    class EntityService : IEntityService
    {
        private readonly IEntityDao _entityDao;
        private IWorkspace _resoureceWorkspace;

        [ImportingConstructor]
        public EntityService(IEntityDao entityDao)
        {
            _entityDao = entityDao;
        }

        public void UpdateEntityScreenItems(EntityScreen entityScreen, int pageNo)
        {
            _entityDao.UpdateEntityScreenItems(entityScreen, pageNo);
        }

        public IEnumerable<EntityScreenItem> GetCurrentEntityScreenItems(EntityScreen entityScreen, int currentPageNo, string entityStateFilter)
        {
            UpdateEntityScreenItems(entityScreen, currentPageNo);
            if (entityScreen != null)
            {
                if (entityScreen.PageCount > 1)
                {
                    return entityScreen.ScreenItems
                         .OrderBy(x => x.SortOrder)
                         .Where(x => string.IsNullOrEmpty(entityStateFilter) || x.EntityState == entityStateFilter)
                         .Skip(entityScreen.ItemCountPerPage * currentPageNo)
                         .Take(entityScreen.ItemCountPerPage);
                }
                return entityScreen.ScreenItems.Where(x => string.IsNullOrEmpty(entityStateFilter) || x.EntityState == entityStateFilter);
            }
            return new List<EntityScreenItem>();
        }

        public IEnumerable<Entity> GetEntitiesByState(string state, int entityTypeId)
        {
            return _entityDao.GetEntitiesByState(state, entityTypeId);
        }

        public IList<Widget> LoadWidgets(string selectedEntityScreen)
        {
            if (_resoureceWorkspace != null)
            {
                _resoureceWorkspace.CommitChanges();
            }
            _resoureceWorkspace = WorkspaceFactory.Create();
            return _resoureceWorkspace.Single<EntityScreen>(x => x.Name == selectedEntityScreen).Widgets;
        }

        public void AddWidgetToEntityScreen(string entityScreenName, Widget widget)
        {
            if (_resoureceWorkspace == null) return;
            _resoureceWorkspace.Single<EntityScreen>(x => x.Name == entityScreenName).Widgets.Add(widget);
            _resoureceWorkspace.CommitChanges();
        }

        public void UpdateEntityScreen(EntityScreen entityScreen)
        {
            UpdateEntityScreenItems(entityScreen, 0);
        }

        public void RemoveWidget(Widget widget)
        {
            if (_resoureceWorkspace == null) return;
            _resoureceWorkspace.Delete<Widget>(x => x.Id == widget.Id);
            _resoureceWorkspace.CommitChanges();
        }

        public List<Entity> SearchEntities(EntityType selectedEntityType, string searchString, string stateFilter)
        {
            if (searchString.Contains(":"))
            {
                var parts = searchString.Split(new[] { ':' }, 2);
                return _entityDao.FindEntities(selectedEntityType, parts[0], parts[1], stateFilter);
            }
            return _entityDao.FindEntities(selectedEntityType, searchString, stateFilter);
        }

        public IList<EntityScreenItem> LoadEntityScreenItems(string selectedEntityScreen)
        {
            if (_resoureceWorkspace != null)
            {
                _resoureceWorkspace.CommitChanges();
            }
            _resoureceWorkspace = WorkspaceFactory.Create();
            return _resoureceWorkspace.Single<EntityScreen>(x => x.Name == selectedEntityScreen).ScreenItems;
        }

        public void SaveEntityScreenItems()
        {
            if (_resoureceWorkspace != null)
            {
                _resoureceWorkspace.CommitChanges();
                _resoureceWorkspace = null;
            }
        }

    }
}
