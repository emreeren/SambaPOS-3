using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Entities;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Specification;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.EntityModule
{
    [Export(typeof(BatchEntityEditorViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    class BatchEntityEditorViewModel : VisibleViewModelBase
    {
        private IEnumerable<EntityType> _entityTypes;
        private EntityType _selectedEntityType;
        private ObservableCollection<EntityListerRow> _entities;
        private string _searchValue;

        public ICaptionCommand SaveCommand { get; set; }

        public BatchEntityEditorViewModel()
        {
            Commands = new List<ICaptionCommand>();
            SaveCommand = new CaptionCommand<string>(Resources.SaveChanges, OnSave, CanSave);
            Commands.Add(SaveCommand);
        }

        private bool CanSave(string arg)
        {
            return Entities.Any(x => x.IsModified);
        }

        private void OnSave(string obj)
        {
            foreach (var entityRow in Entities.Where(x => x.IsModified))
            {
                Entity entity = entityRow.Model;
                if (Dao.Exists<Entity>(x => x.Name == entity.Name && (x.Id != entity.Id))) continue;
                Dao.Save(entity);
            }
            RefreshItems();
        }

        public IEnumerable<EntityType> EntityTypes
        {
            get { return _entityTypes ?? (_entityTypes = Dao.Query<EntityType>(x => x.EntityCustomFields)); }
        }

        public EntityType SelectedEntityType
        {
            get { return _selectedEntityType; }
            set
            {
                _selectedEntityType = value;
                SearchValue = "";
                RefreshItems();
                RaisePropertyChanged(() => SelectedEntityType);
            }
        }

        public ObservableCollection<EntityListerRow> Entities
        {
            get { return _entities; }
            set { _entities = value; RaisePropertyChanged(() => Entities); }
        }

        public bool DisplayLimitWarning { get; set; }

        public string SearchValue
        {
            get { return _searchValue; }
            set { _searchValue = value; RaisePropertyChanged(() => SearchValue); }
        }

        public IList<ICaptionCommand> Commands { get; set; }

        public override void OnShown()
        {
            Entities = new ObservableCollection<EntityListerRow>();
            SelectedEntityType = EntityTypes.FirstOrDefault();
            base.OnShown();
        }

        protected override string GetHeaderInfo()
        {
            return Resources.BatchEntityEditor;
        }

        public override Type GetViewType()
        {
            return typeof(BatchEntityEditorView);
        }

        public void RefreshItems()
        {
            Expression<Func<Entity, bool>> predictate = x => x.EntityTypeId == SelectedEntityType.Id;
            if (!string.IsNullOrWhiteSpace(SearchValue))
                predictate = predictate.And(x => x.Name.Contains(SearchValue));
            var entities = Dao.Query(predictate).Select(x => new EntityListerRow(x));
            Entities = new ObservableCollection<EntityListerRow>(entities);
        }
    }

    internal class EntityListerRow : ObservableObject
    {
        private bool _isModified;

        public EntityListerRow(Entity entity)
        {
            Model = entity;
        }

        public Entity Model { get; set; }

        public string Name
        {
            get { return Model.Name; }
            set
            {
                Model.Name = value;
                IsModified = true;
            }
        }
        public bool IsModified
        {
            get { return _isModified; }
            set { _isModified = value; RaisePropertyChanged(() => IsModified); }
        }

        public string this[string fieldName]
        {
            get
            {
                return Model.GetCustomData(fieldName);
            }
            set
            {
                Model.SetCustomData(fieldName, value);
                IsModified = true;
            }
        }
    }
}
