using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.EntityModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class EntityTypeViewModel : EntityViewModelBase<EntityType>
    {
        [ImportingConstructor]
        public EntityTypeViewModel()
        {
            AddCustomFieldCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.CustomField), OnAddCustomField);
            DeleteCustomFieldCommand = new CaptionCommand<EntityCustomFieldViewModel>(string.Format(Resources.Delete_f, Resources.CustomField), OnDeleteCustomField, CanDeleteCustomField);
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

        public string PrimaryFieldName { get { return Model.PrimaryFieldName; } set { Model.PrimaryFieldName = value; } }
        public string PrimaryFieldFormat { get { return Model.PrimaryFieldFormat; } set { Model.PrimaryFieldFormat = value; } }
        public string AccountBalanceDisplayFormat { get { return Model.AccountBalanceDisplayFormat; } set { Model.AccountBalanceDisplayFormat = value; } }
        public string DisplayFormat { get { return Model.DisplayFormat; } set { Model.DisplayFormat = value; } }
        public ICaptionCommand AddCustomFieldCommand { get; set; }
        public ICaptionCommand DeleteCustomFieldCommand { get; set; }

        public EntityCustomFieldViewModel SelectedCustomField { get; set; }

        private ObservableCollection<EntityCustomFieldViewModel> _entityCustomFields;
        public ObservableCollection<EntityCustomFieldViewModel> EntityCustomFields
        {
            get { return _entityCustomFields ?? (_entityCustomFields = new ObservableCollection<EntityCustomFieldViewModel>(Model.EntityCustomFields.Select(x => new EntityCustomFieldViewModel(x)))); }
        }

        private bool CanDeleteCustomField(EntityCustomFieldViewModel arg)
        {
            return SelectedCustomField != null;
        }

        private void OnDeleteCustomField(EntityCustomFieldViewModel accountCustomFieldViewModel)
        {
            if (SelectedCustomField != null)
            {
                Model.EntityCustomFields.Remove(SelectedCustomField.Model);
                if (SelectedCustomField.Model.Id > 0)
                    Workspace.Delete(SelectedCustomField.Model);
                EntityCustomFields.Remove(SelectedCustomField);
            }
        }

        private void OnAddCustomField(string s)
        {
            var result = Model.AddCustomField(string.Format(Resources.New_f, Resources.CustomField), 0);
            EntityCustomFields.Add(new EntityCustomFieldViewModel(result));
        }

        public override string GetModelTypeString()
        {
            return Resources.EntityType;
        }

        public override Type GetViewType()
        {
            return typeof(EntityTypeView);
        }
    }
}
