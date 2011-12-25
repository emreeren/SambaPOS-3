using System;
using System.Collections.Generic;
using Samba.Domain.Models.Locations;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.LocationModule
{
    public class LocationEditorViewModel : EntityViewModelBase<Location>
    {
        private IEnumerable<string> _categories;
        public IEnumerable<string> Categories { get { return _categories ?? (_categories = Dao.Distinct<Location>(x => x.Category)); } }

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

        protected override string GetSaveErrorMessage()
        {
            if (Dao.Single<Location>(x => x.Name.ToLower() == Model.Name.ToLower() && x.Id != Model.Id) != null)
                return Resources.SaveErrorDuplicateLocationName;
            return base.GetSaveErrorMessage();
        }
    }
}
