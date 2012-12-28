using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class TransactionTypeViewModel : EntityViewModelBase<InventoryTransactionType>
    {
        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes
        {
            get { return _accountTransactionTypes ?? (_accountTransactionTypes = Workspace.All<AccountTransactionType>()); }
        }

        public AccountTransactionType AccountTransactionType { get { return Model.AccountTransactionType; } set { Model.AccountTransactionType = value; } }

        private IEnumerable<ResourceType> _resourceTypes;
        public IEnumerable<ResourceType> ResourceTypes
        {
            get { return _resourceTypes ?? (_resourceTypes = Workspace.All<ResourceType>()); }
        }

        private ResourceType _sourceResourceType;
        public ResourceType SourceResourceType
        {
            get
            {
                return _sourceResourceType ??
                       (_sourceResourceType = ResourceTypes.SingleOrDefault(x => x.Id == Model.SourceResourceTypeId));
            }
            set
            {
                Model.SourceResourceTypeId = value.Id;
                _sourceResourceType = null;
                _sourceResources = null;
                RaisePropertyChanged(() => SourceResourceType);
                RaisePropertyChanged(() => SourceResources);
            }
        }

        private ResourceType _targetResourceType;
        public ResourceType TargetResourceType
        {
            get
            {
                return _targetResourceType ??
                       (_targetResourceType = ResourceTypes.SingleOrDefault(x => x.Id == Model.TargetResourceTypeId));
            }
            set
            {
                Model.TargetResourceTypeId = value.Id;
                _targetResourceType = null;
                _targetResources = null;
                RaisePropertyChanged(() => TargetResourceType);
                RaisePropertyChanged(() => TargetResources);
            }
        }

        public int? DefaultSourceResourceId { get { return Model.DefaultSourceResourceId; } set { Model.DefaultSourceResourceId = value.GetValueOrDefault(0); } }
        public int? DefaultTargetResourceId { get { return Model.DefaultTargetResourceId; } set { Model.DefaultTargetResourceId = value.GetValueOrDefault(0); } }

        private IEnumerable<Resource> _sourceResources;
        public IEnumerable<Resource> SourceResources
        {
            get { return _sourceResources ?? (_sourceResources = GetSoruceResources()); }
        }

        private IEnumerable<Resource> _targetResources;
        public IEnumerable<Resource> TargetResources
        {
            get { return _targetResources ?? (_targetResources = GetTargetResources()); }
        }

        private IEnumerable<Resource> GetSoruceResources()
        {
            return SourceResourceType != null ? Workspace.All<Resource>(x => x.ResourceTypeId == SourceResourceType.Id).ToList() : null;
        }

        private IEnumerable<Resource> GetTargetResources()
        {
            return TargetResourceType != null ? Workspace.All<Resource>(x => x.ResourceTypeId == TargetResourceType.Id).ToList() : null;
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
            RuleFor(x => x.SourceResourceTypeId).GreaterThan(0);
            RuleFor(x => x.TargetResourceTypeId).GreaterThan(0);
            RuleFor(x => x.DefaultSourceResourceId).NotEqual(x => x.DefaultTargetResourceId).When(
                x => x.DefaultSourceResourceId > 0 && x.DefaultTargetResourceId > 0);
        }
    }
}
