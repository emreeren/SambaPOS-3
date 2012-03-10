using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using System.Linq;
using Samba.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export(typeof(AccountScreenItemViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountScreenItemViewModel : EntityViewModelBase<AccountScreenItem>, IEntityCreator<AccountScreenItem>
    {
        private readonly ILocationService _locationService;

        [ImportingConstructor]
        public AccountScreenItemViewModel(ILocationService locationService)
        {
            _locationService = locationService;
        }

        private IEnumerable<string> _categories;
        public IEnumerable<string> Categories { get { return _categories ?? (_categories = _locationService.GetCategories()); } }

        public string Category { get { return Model.Category; } set { Model.Category = value; } }
        public string GroupValue { get { return Model.Category; } }

        public override Type GetViewType()
        {
            return typeof(AccountScreenItemView);
        }

        public override string GetModelTypeString()
        {
            return Resources.AccountScreenItem;
        }

        protected override bool CanSave(string arg)
        {
            return Model.TicketId <= 0 && base.CanSave(arg);
        }

        public IEnumerable<AccountScreenItem> CreateItems(IEnumerable<string> data)
        {
            return new DataCreationService().BatchCreateLocations(data.ToArray(), Workspace);
        }
    }
}
