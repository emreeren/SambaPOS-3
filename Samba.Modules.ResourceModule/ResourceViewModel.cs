using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.ResourceModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class ResourceViewModel : EntityViewModelBase<Resource>,IEntityCreator<Resource>
    {
        private IEnumerable<ResourceTemplate> _accountTemplates;
        public IEnumerable<ResourceTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = Workspace.All<ResourceTemplate>()); }
        }

        private ResourceTemplate _accountTemplate;
        public ResourceTemplate AccountTemplate
        {
            get
            {
                return _accountTemplate ??
                       (_accountTemplate = Workspace.Single<ResourceTemplate>(x => x.Id == Model.ResourceTemplateId));
            }
            set
            {
                Model.ResourceTemplateId = value.Id;
                _accountTemplate = null;
                _customDataViewModel = null;
                RaisePropertyChanged(() => CustomDataViewModel);
                RaisePropertyChanged(() => AccountTemplate);
            }
        }

        private ResourceCustomDataViewModel _customDataViewModel;
        public ResourceCustomDataViewModel CustomDataViewModel
        {
            get { return _customDataViewModel ?? (_customDataViewModel = Model != null ? new ResourceCustomDataViewModel(Model, AccountTemplate) : null); }
        }

        public override Type GetViewType()
        {
            return typeof(ResourceView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Account;
        }

        public string SearchString { get { return Model.SearchString; } set { Model.SearchString = value; } }

        protected override AbstractValidator<Resource> GetValidator()
        {
            return new AccountValidator();
        }

        protected override void OnSave(string value)
        {
            CustomDataViewModel.Update();
            base.OnSave(value);
        } 

        public IEnumerable<Resource> CreateItems(IEnumerable<string> data)
        { 
            return new DataCreationService().BatchCreateResources(data.ToArray(), Workspace);
        }
    }

    internal class AccountValidator : EntityValidator<Resource>
    {
        public AccountValidator()
        {
            RuleFor(x => x.ResourceTemplateId).GreaterThan(0);
        }
    }
}
