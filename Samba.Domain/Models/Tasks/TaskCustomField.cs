using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tasks
{
    public class TaskCustomField : EntityClass
    {
        public int TaskTypeId { get; set; }
        public int FieldType { get; set; }
        public string EditingFormat { get; set; }
        public string DisplayFormat { get; set; }

        public string GetFormattedValue(string value)
        {
            if (!string.IsNullOrEmpty(DisplayFormat)) return DisplayFormat.Replace("#", value);
            return value;
        }
    }
}