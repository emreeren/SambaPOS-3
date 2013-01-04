using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.ResourceModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class ResourceTypeViewModel : EntityViewModelBase<ResourceType>
    {
        [ImportingConstructor]
        public ResourceTypeViewModel()
        {
            AddCustomFieldCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.CustomField), OnAddCustomField);
            DeleteCustomFieldCommand = new CaptionCommand<ResourceCustomFieldViewModel>(string.Format(Resources.Delete_f, Resources.CustomField), OnDeleteCustomField, CanDeleteCustomField);
        }

        public string AccountNameTemplate { get { return Model.AccountNameTemplate; } set { Model.AccountNameTemplate = value; } }
        public string EntityName { get { return Model.EntityName; } set { Model.EntityName = value; } }

        private IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes { get { return _accountTypes ?? (_accountTypes = Workspace.All<AccountType>()); } }

        public AccountType AccountType
        {
            get { return AccountTypes.SingleOrDefault(x => x.Id == Model.AccountTypeId); }
            set { Model.AccountTypeId = value != null ? value.Id : 0; }
        }

        private IEnumerable<WarehouseType> _warehouseTypes;
        public IEnumerable<WarehouseType> WarehouseTypes
        {
            get { return _warehouseTypes ?? (_warehouseTypes = Workspace.All<WarehouseType>()); }
        }

        public WarehouseType WarehouseType
        {
            get { return WarehouseTypes.SingleOrDefault(x => x.Id == Model.WarehouseTypeId); }
            set { Model.WarehouseTypeId = value != null ? value.Id : 0; }
        }

        public ICaptionCommand AddCustomFieldCommand { get; set; }
        public ICaptionCommand DeleteCustomFieldCommand { get; set; }

        public ResourceCustomFieldViewModel SelectedCustomField { get; set; }

        private ObservableCollection<ResourceCustomFieldViewModel> _resourceCustomFields;
        public ObservableCollection<ResourceCustomFieldViewModel> ResourceCustomFields
        {
            get { return _resourceCustomFields ?? (_resourceCustomFields = new ObservableCollection<ResourceCustomFieldViewModel>(Model.ResoruceCustomFields.Select(x => new ResourceCustomFieldViewModel(x)))); }
        }

        private bool CanDeleteCustomField(ResourceCustomFieldViewModel arg)
        {
            return SelectedCustomField != null;
        }

        private void OnDeleteCustomField(ResourceCustomFieldViewModel accountCustomFieldViewModel)
        {
            if (SelectedCustomField != null)
            {
                Model.ResoruceCustomFields.Remove(SelectedCustomField.Model);
                if (SelectedCustomField.Model.Id > 0)
                    Workspace.Delete(SelectedCustomField.Model);
                ResourceCustomFields.Remove(SelectedCustomField);
            }
        }

        private void OnAddCustomField(string s)
        {
            var result = Model.AddCustomField(string.Format(Resources.New_f, Resources.CustomField), 0);
            ResourceCustomFields.Add(new ResourceCustomFieldViewModel(result));
        }

        public override string GetModelTypeString()
        {
            return Resources.ResourceType;
        }

        public override Type GetViewType()
        {
            return typeof(ResourceTypeView);
        }
    }
}
