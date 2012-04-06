using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class ResourceTemplate : Entity,IOrderable
    {
        public int Order { get; set; }
        public string EntityName { get; set; }

        private readonly IList<ResourceCustomField> _resourceCustomFields;
        public virtual IList<ResourceCustomField> ResoruceCustomFields
        {
            get { return _resourceCustomFields; }
        }

        public ResourceTemplate()
        {
            _resourceCustomFields = new List<ResourceCustomField>();
        }

        public ResourceCustomField AddCustomField(string fieldName, int fieldType)
        {
            var result = new ResourceCustomField { Name = fieldName, FieldType = fieldType };
            _resourceCustomFields.Add(result);
            return result;
        }

        public IEnumerable<ResourceCustomField> GetMatchingFields(Resource resource, string searchString)
        {
            if (!string.IsNullOrEmpty(resource.CustomData))
            {
                const string nameFormat = "\"Name\":\"{0}\"";
                var data = resource.CustomData.Split(new[] { "}," }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => Regex.Match(x, "\"Value\":\"([^\"]+)\"").Groups[1].Value.ToLower().Contains(searchString.ToLower()));
                return ResoruceCustomFields.Where(x => data.Any(y => y.ToLower().Contains(string.Format(nameFormat, x.Name).ToLower()) && y.ToLower().Contains(searchString.ToLower())));
            }
            return ResoruceCustomFields;
        }

        public string UserString
        {
            get { return Name; }
        }
    }
}
