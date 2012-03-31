using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;

namespace Samba.Modules.ResourceModule
{
    public class ResourceCustomFieldViewModel : ObservableObject
    {
        public ResourceCustomField Model { get; set; }

        private string[] _fieldTypes;
        public string[] FieldTypes
        {
            get { return _fieldTypes ?? (_fieldTypes = new[] { "String", "WideString", "Number" }); }
        }

        public string FieldType
        {
            get { return FieldTypes[Model.FieldType]; }
            set { Model.FieldType = FieldTypes.ToList().IndexOf(value); }
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
        public bool Hidden { get { return Model.Hidden; } set { Model.Hidden = value; } }
        public string EditingFormat { get { return Model.EditingFormat; } set { Model.EditingFormat = value; } }
        public string ValueSource { get { return Model.ValueSource; } set { Model.ValueSource = value; } }

        public ResourceCustomFieldViewModel(ResourceCustomField model)
        {
            Model = model;
        }
    }
}
