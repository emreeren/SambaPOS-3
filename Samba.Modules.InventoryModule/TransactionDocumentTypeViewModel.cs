using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    class TransactionDocumentTypeViewModel : EntityViewModelBase<InventoryTransactionDocumentType>
    {
        private IEnumerable<InventoryTransactionType> _inventoryTransactionTypes;
        public IEnumerable<InventoryTransactionType> InventoryTransactionTypes
        {
            get { return _inventoryTransactionTypes ?? (_inventoryTransactionTypes = Workspace.All<InventoryTransactionType>()); }
        }

        public InventoryTransactionType InventoryTransactionType { get { return Model.InventoryTransactionType; } set { Model.InventoryTransactionType = value; } }

        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes
        {
            get { return _accountTransactionTypes ?? (_accountTransactionTypes = Workspace.All<AccountTransactionType>()); }
        }

        public AccountTransactionType AccountTransactionType { get { return Model.AccountTransactionType; } set { Model.AccountTransactionType = value; } }

        private IEnumerable<EntityType> _entityTypes;
        public IEnumerable<EntityType> EntityTypes
        {
            get { return _entityTypes ?? (_entityTypes = Workspace.All<EntityType>()); }
        }

        private EntityType _sourceEntityType;
        public EntityType SourceEntityType
        {
            get
            {
                return _sourceEntityType ??
                       (_sourceEntityType = EntityTypes.SingleOrDefault(x => x.Id == Model.SourceEntityTypeId));
            }
            set
            {
                Model.SourceEntityTypeId = value.Id;
                _sourceEntityType = null;
                _sourceEntities = null;
                RaisePropertyChanged(() => SourceEntityType);
                RaisePropertyChanged(() => SourceEntities);
            }
        }

        private EntityType _targetEntityType;
        public EntityType TargetEntityType
        {
            get
            {
                return _targetEntityType ??
                       (_targetEntityType = EntityTypes.SingleOrDefault(x => x.Id == Model.TargetEntityTypeId));
            }
            set
            {
                Model.TargetEntityTypeId = value.Id;
                _targetEntityType = null;
                _targetEntities = null;
                RaisePropertyChanged(() => TargetEntityType);
                RaisePropertyChanged(() => TargetEntities);
            }
        }

        public int? DefaultSourceEntityId { get { return Model.DefaultSourceEntityId; } set { Model.DefaultSourceEntityId = value.GetValueOrDefault(0); } }
        public int? DefaultTargetEntityId { get { return Model.DefaultTargetEntityId; } set { Model.DefaultTargetEntityId = value.GetValueOrDefault(0); } }

        private IEnumerable<Entity> _sourceEntities;
        public IEnumerable<Entity> SourceEntities
        {
            get { return _sourceEntities ?? (_sourceEntities = GetSoruceEntities()); }
        }

        private IEnumerable<Entity> _targetEntities;
        public IEnumerable<Entity> TargetEntities
        {
            get { return _targetEntities ?? (_targetEntities = GetTargetEntities()); }
        }

        private IEnumerable<Entity> GetSoruceEntities()
        {
            return SourceEntityType != null ? Workspace.All<Entity>(x => x.EntityTypeId == SourceEntityType.Id).ToList() : null;
        }

        private IEnumerable<Entity> GetTargetEntities()
        {
            return TargetEntityType != null ? Workspace.All<Entity>(x => x.EntityTypeId == TargetEntityType.Id).ToList() : null;
        }

        public override Type GetViewType()
        {
            return typeof(TransactionDocumentTypeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.DocumentType;
        }
    }
}
