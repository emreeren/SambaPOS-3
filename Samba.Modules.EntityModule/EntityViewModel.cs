using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Inventory;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common.DataGeneration;
using Samba.Services;

namespace Samba.Modules.EntityModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class EntityViewModel : EntityViewModelBase<Entity>, IEntityCreator<Entity>
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public EntityViewModel(IAccountService accountService, ICacheService cacheService)
        {
            _accountService = accountService;
            _cacheService = cacheService;
        }

        private IEnumerable<EntityType> _entityTypes;
        public IEnumerable<EntityType> EntityTypes
        {
            get { return _entityTypes ?? (_entityTypes = _cacheService.GetEntityTypes()); }
        }

        private EntityType _entityType;
        public EntityType EntityType
        {
            get
            {
                return _entityType ?? (_entityType = _cacheService.GetEntityTypeById(Model.EntityTypeId));
            }
            set
            {
                Model.EntityTypeId = value.Id;
                _entityType = null;
                _customDataViewModel = null;
                RaisePropertyChanged(() => CustomDataViewModel);
                RaisePropertyChanged(() => EntityType);
            }
        }

        private EntityCustomDataViewModel _customDataViewModel;
        public EntityCustomDataViewModel CustomDataViewModel
        {
            get { return _customDataViewModel ?? (_customDataViewModel = Model != null ? new EntityCustomDataViewModel(Model, EntityType) : null); }
        }

        private string _accountName;
        public string AccountName
        {
            get { return _accountName ?? (_accountName = _accountService.GetAccountNameById(Model.AccountId)); }
            set
            {
                _accountName = value;
                Model.AccountId = _accountService.GetAccountIdByName(value);
                if (Model.AccountId == 0)
                    RaisePropertyChanged(() => AccountNames);
                _accountName = null;
                RaisePropertyChanged(() => AccountName);
            }
        }

        public IEnumerable<string> AccountNames
        {
            get
            {
                if (EntityType == null) return null;
                return _accountService.GetCompletingAccountNames(EntityType.AccountTypeId, AccountName);
            }
        }

        public Warehouse Warehouse
        {
            get { return Warehouses.SingleOrDefault(x => x.Id == Model.WarehouseId); }
            set { Model.WarehouseId = value != null ? value.Id : 0; }
        }

        public IEnumerable<Warehouse> Warehouses
        {
            get
            {
                var whId = EntityType != null ? EntityType.WarehouseTypeId : 0;
                return Workspace.Query<Warehouse>(x => x.WarehouseTypeId == whId);
            }
        }

        public string GroupValue { get { return NameCache.GetName<EntityType>(Model.EntityTypeId); } }

        public override Type GetViewType()
        {
            return typeof(EntityView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Entity;
        }

        protected override AbstractValidator<Entity> GetValidator()
        {
            return new EntityValidator();
        }

        protected override void OnSave(string value)
        {
            CustomDataViewModel.Update();
            if (Model.Id > 0)
            {
                var screenItems = Workspace.All<EntityScreenItem>(x => x.EntityId == Model.Id);
                foreach (var entityScreenItem in screenItems)
                {
                    entityScreenItem.Name = Model.Name;
                }
            }

            base.OnSave(value);
        }

        public IEnumerable<Entity> CreateItems(IEnumerable<string> data)
        {
            return new DataCreationService().BatchCreateEntities(data.ToArray(), Workspace);
        }
    }

    internal class EntityValidator : EntityValidator<Entity>
    {
        public EntityValidator()
        {
            RuleFor(x => x.EntityTypeId).GreaterThan(0);
        }
    }
}
