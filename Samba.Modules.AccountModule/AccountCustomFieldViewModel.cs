using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule
{
    public class AccountCustomFieldViewModel : ObservableObject
    {
        public AccountCustomField Model { get; set; }

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

        public string EditingFormat { get { return Model.EditingFormat; } set { Model.EditingFormat = value; } }
        public string ValueSource { get { return Model.ValueSource; } set { Model.ValueSource = value; } }

        public AccountCustomFieldViewModel(AccountCustomField model)
        {
            Model = model;
        }
    }
}
