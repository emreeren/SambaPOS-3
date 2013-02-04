using System.Collections.Generic;
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
        public IEnumerable<string> Values { get { return (ValueSource ?? "").Split(',').Select(x => x.Trim()); } }
    }
}
