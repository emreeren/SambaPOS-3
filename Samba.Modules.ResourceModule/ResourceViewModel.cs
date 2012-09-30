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
    public class ResourceViewModel : EntityViewModelBase<Resource>, IEntityCreator<Resource>
    {
        private readonly IAccountService _accountService;

        [ImportingConstructor]
        public ResourceViewModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        private IEnumerable<ResourceType> _resourceTypes;
        public IEnumerable<ResourceType> ResourceTypes
        {
            get { return _resourceTypes ?? (_resourceTypes = Workspace.All<ResourceType>()); }
        }

        private ResourceType _resoureTemplate;
        public ResourceType ResourceType
        {
            get
            {
                return _resoureTemplate ??
                       (_resoureTemplate = Workspace.Single<ResourceType>(x => x.Id == Model.ResourceTypeId));
            }
            set
            {
                Model.ResourceTypeId = value.Id;
                _resoureTemplate = null;
                _customDataViewModel = null;
                RaisePropertyChanged(() => CustomDataViewModel);
                RaisePropertyChanged(() => ResourceType);
            }
        }

        private ResourceCustomDataViewModel _customDataViewModel;
        public ResourceCustomDataViewModel CustomDataViewModel
        {
            get { return _customDataViewModel ?? (_customDataViewModel = Model != null ? new ResourceCustomDataViewModel(Model, ResourceType) : null); }
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
                if (ResourceType == null) return null;
                return _accountService.GetCompletingAccountNames(ResourceType.AccountTypeId, AccountName);
            }
        }

        public string GroupValue { get { return NameCache.GetName<ResourceType>(Model.ResourceTypeId); } }

        public override Type GetViewType()
        {
            return typeof(ResourceView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Resource;
        }

        public string SearchString { get { return Model.SearchString; } set { Model.SearchString = value; } }

        protected override AbstractValidator<Resource> GetValidator()
        {
            return new ResourceValidator();
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

    internal class ResourceValidator : EntityValidator<Resource>
    {
        public ResourceValidator()
        {
            RuleFor(x => x.ResourceTypeId).GreaterThan(0);
        }
    }
}
