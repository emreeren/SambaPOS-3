using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using System.Linq;
using Samba.Services;

namespace Samba.Modules.ResourceModule
{
    [Export(typeof(ResourceScreenItemViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ResourceScreenItemViewModel : EntityViewModelBase<ResourceScreenItem>, IEntityCreator<ResourceScreenItem>
    {
        private readonly ILocationService _locationService;

        [ImportingConstructor]
        public ResourceScreenItemViewModel(ILocationService locationService)
        {
            _locationService = locationService;
        }

        private IEnumerable<string> _categories;
        public IEnumerable<string> Categories { get { return _categories ?? (_categories = _locationService.GetCategories()); } }

        public string Category { get { return Model.Category; } set { Model.Category = value; } }
        public string GroupValue { get { return Model.Category; } }

        public override Type GetViewType()
        {
            return typeof(ResourceScreenItemView);
        }

        public override string GetModelTypeString()
        {
            return Resources.ResourceScreenItem;
        }

        public IEnumerable<ResourceScreenItem> CreateItems(IEnumerable<string> data)
        {
            return new DataCreationService().BatchCreateLocations(data.ToArray(), Workspace);
        }
    }
}
