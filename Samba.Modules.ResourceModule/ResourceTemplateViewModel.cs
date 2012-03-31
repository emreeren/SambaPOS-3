using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.ResourceModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class ResourceTemplateViewModel : EntityViewModelBase<ResourceTemplate>
    {
        [ImportingConstructor]
        public ResourceTemplateViewModel()
        {
            AddCustomFieldCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.CustomField), OnAddCustomField);
            DeleteCustomFieldCommand = new CaptionCommand<ResourceCustomFieldViewModel>(string.Format(Resources.Delete_f, Resources.CustomField), OnDeleteCustomField, CanDeleteCustomField);
        }

        public string EntityName { get { return Model.EntityName; } set { Model.EntityName = value; } }

        public ICaptionCommand AddCustomFieldCommand { get; set; }
        public ICaptionCommand DeleteCustomFieldCommand { get; set; }

        public ResourceCustomFieldViewModel SelectedCustomField { get; set; }

        private ObservableCollection<ResourceCustomFieldViewModel> _accountCustomFields;
        public ObservableCollection<ResourceCustomFieldViewModel> AccountCustomFields
        {
            get { return _accountCustomFields ?? (_accountCustomFields = new ObservableCollection<ResourceCustomFieldViewModel>(Model.ResoruceCustomFields.Select(x => new ResourceCustomFieldViewModel(x)))); }
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
                AccountCustomFields.Remove(SelectedCustomField);
            }
        }

        private void OnAddCustomField(string s)
        {
            var result = Model.AddCustomField(string.Format(Resources.New_f, Resources.CustomField), 0);
            AccountCustomFields.Add(new ResourceCustomFieldViewModel(result));
        }

        public override string GetModelTypeString()
        {
            return Resources.AccountTemplate;
        }

        public override Type GetViewType()
        {
            return typeof(ResourceTemplateView);
        }
    }
}
