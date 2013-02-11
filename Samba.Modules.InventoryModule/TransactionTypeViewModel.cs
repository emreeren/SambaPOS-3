using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class TransactionTypeViewModel : EntityViewModelBase<InventoryTransactionType>
    {

        private IEnumerable<WarehouseType> _warehouseTypes;
        public IEnumerable<WarehouseType> WarehouseTypes
        {
            get { return _warehouseTypes ?? (_warehouseTypes = Workspace.All<WarehouseType>()); }
        }

        private WarehouseType _sourceWarehouseType;
        public WarehouseType SourceWarehouseType
        {
            get
            {
                return _sourceWarehouseType ??
                       (_sourceWarehouseType = WarehouseTypes.SingleOrDefault(x => x.Id == Model.SourceWarehouseTypeId));
            }
            set
            {
                Model.SourceWarehouseTypeId = value != null ? value.Id : 0;
                _sourceWarehouseType = null;
                _sourceWarehouses = null;
                RaisePropertyChanged(() => SourceWarehouseType);
                RaisePropertyChanged(() => SourceWarehouses);
            }
        }

        private WarehouseType _targetWarehouseType;
        public WarehouseType TargetWarehouseType
        {
            get
            {
                return _targetWarehouseType ??
                       (_targetWarehouseType = WarehouseTypes.SingleOrDefault(x => x.Id == Model.TargetWarehouseTypeId));
            }
            set
            {
                Model.TargetWarehouseTypeId = value != null ? value.Id : 0;
                _targetWarehouseType = null;
                _targetWarehouses = null;
                RaisePropertyChanged(() => TargetWarehouseType);
                RaisePropertyChanged(() => TargetWarehouses);
            }
        }

        public int? DefaultSourceWarehouseId { get { return Model.DefaultSourceWarehouseId; } set { Model.DefaultSourceWarehouseId = value.GetValueOrDefault(0); } }
        public int? DefaultTargetWarehouseId { get { return Model.DefaultTargetWarehouseId; } set { Model.DefaultTargetWarehouseId = value.GetValueOrDefault(0); } }

        private IEnumerable<Warehouse> _sourceWarehouses;
        public IEnumerable<Warehouse> SourceWarehouses
        {
            get { return _sourceWarehouses ?? (_sourceWarehouses = GetSoruceWarehouses()); }
        }

        private IEnumerable<Warehouse> _targetWarehouses;
        public IEnumerable<Warehouse> TargetWarehouses
        {
            get { return _targetWarehouses ?? (_targetWarehouses = GetTargetWarehouses()); }
        }

        private IEnumerable<Warehouse> GetSoruceWarehouses()
        {
            return SourceWarehouseType != null ? Workspace.All<Warehouse>(x => x.WarehouseTypeId == SourceWarehouseType.Id).ToList() : null;
        }

        private IEnumerable<Warehouse> GetTargetWarehouses()
        {
            return TargetWarehouseType != null ? Workspace.All<Warehouse>(x => x.WarehouseTypeId == TargetWarehouseType.Id).ToList() : null;
        }

        protected override AbstractValidator<InventoryTransactionType> GetValidator()
        {
            return new TransactionTypeValidator();
        }

        public override Type GetViewType()
        {
            return typeof(TransactionTypeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.TransactionType;
        }
    }

    internal class TransactionTypeValidator : EntityValidator<InventoryTransactionType>
    {
        public TransactionTypeValidator()
        {
            RuleFor(x => x.SourceWarehouseTypeId)
                .GreaterThan(0)
                .When(x => x.TargetWarehouseTypeId == 0);
            RuleFor(x => x.DefaultSourceWarehouseId)
                .NotEqual(x => x.DefaultTargetWarehouseId)
                .When(x => x.DefaultSourceWarehouseId > 0 && x.DefaultTargetWarehouseId > 0);
        }
    }
}
