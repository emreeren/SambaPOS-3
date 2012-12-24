using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class WarehouseViewModel : EntityViewModelBase<Warehouse>
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public WarehouseViewModel(IAccountService accountService, ICacheService cacheService)
        {
            _accountService = accountService;
            _cacheService = cacheService;
        }

        private IEnumerable<WarehouseType> _warehouseTypes;
        public IEnumerable<WarehouseType> WarehouseTypes
        {
            get { return _warehouseTypes ?? (_warehouseTypes = _cacheService.GetWarehouseTypes()); }
        }

        private WarehouseType _warehouseType;
        public WarehouseType WarehouseType
        {
            get
            {
                return _warehouseType ??
                       (_warehouseType = _cacheService.GetWarehouseTypeById(Model.WarehouseTypeId));
            }
            set
            {
                Model.WarehouseTypeId = value.Id;
                _warehouseType = null;
                RaisePropertyChanged(() => WarehouseType);
            }
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
                if (WarehouseType == null) return null;
                return _accountService.GetCompletingAccountNames(WarehouseType.AccountTypeId, AccountName);
            }
        }

        public override Type GetViewType()
        {
            return typeof(WarehouseView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Warehouse;
        }
    }
}
