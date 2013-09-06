using System.Collections.Generic;
using System.IO;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Entities
{
    public class EntityCustomField : EntityClass
    {
        public int FieldType { get; set; }
        public string EditingFormat { get; set; }
        public string ValueSource { get; set; }
        public bool Hidden { get; set; }

        public bool IsString { get { return FieldType == 0; } }
        public bool IsWideString { get { return FieldType == 1; } }
        public bool IsNumber { get { return FieldType == 2; } }
        public bool IsQuery { get { return FieldType == 3; } }
        public bool IsDate { get { return FieldType == 4; } }

        private IEnumerable<string> _values;
        public IEnumerable<string> Values { get { return _values ?? (_values = GetValues()); } }

        private IEnumerable<string> GetValues()
        {
            return File.Exists(ValueSource) ? File.ReadAllLines(ValueSource) : (ValueSource ?? "").Split(',').Select(x => x.Trim());
        }
    }
}
