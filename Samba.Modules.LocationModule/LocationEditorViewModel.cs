using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Locations;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.LocationModule
{
    [Export(typeof(LocationEditorViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class LocationEditorViewModel : EntityViewModelBase<Location>
    {
        private readonly ILocationService _locationService;

        [ImportingConstructor]
        public LocationEditorViewModel(ILocationService locationService)
        {
            _locationService = locationService;
        }

        private IEnumerable<string> _categories;
        public IEnumerable<string> Categories { get { return _categories ?? (_categories = _locationService.GetCategories()); } }

        public string Category { get { return Model.Category; } set { Model.Category = value; } }
        public string GroupValue { get { return Model.Category; } }

        public override Type GetViewType()
        {
            return typeof(LocationEditorView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Location;
        }

        protected override bool CanSave(string arg)
        {
            return Model.TicketId <= 0 && base.CanSave(arg);
        }
    }
}
