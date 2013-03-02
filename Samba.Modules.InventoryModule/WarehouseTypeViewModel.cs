using System;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class WarehouseTypeViewModel : EntityViewModelBase<WarehouseType>
    {
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
