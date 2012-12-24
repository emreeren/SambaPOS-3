using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class WarehouseTypeViewModel : EntityViewModelBase<WarehouseType>
    {
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public WarehouseTypeViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        private IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes
        {
            get { return _accountTypes ?? (_accountTypes = _cacheService.GetAccountTypes()); }
        }

        public int AccountTypeId { get { return Model.AccountTypeId; } set { Model.AccountTypeId = value; } }

        public override Type GetViewType()
        {
            return typeof(WarehouseTypeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.WarehouseType;
        }
    }
}
