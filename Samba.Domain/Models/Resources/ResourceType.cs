using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class ResourceType : Entity, IOrderable
    {
        public int SortOrder { get; set; }
        public string EntityName { get; set; }
        public int AccountTypeId { get; set; }
        public string AccountNameTemplate { get; set; }

        private readonly IList<ResourceCustomField> _resourceCustomFields;
        public virtual IList<ResourceCustomField> ResoruceCustomFields
        {
            get { return _resourceCustomFields; }
        }

        public ResourceType()
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

        public string GenerateAccountName(Resource resource)
        {
            if (string.IsNullOrEmpty(resource.Name)) return "";

            var result = AccountNameTemplate;
            result = result.Replace("[Id]", resource.Id.ToString(CultureInfo.InvariantCulture));
            result = result.Replace("[Name]", resource.Name);
            while (Regex.IsMatch(result, "\\[([^\\]]+)\\]"))
            {
                var match = Regex.Match(result, "\\[([^\\]]+)\\]");
                var propName = match.Groups[1].Value;
                var data = resource.GetCustomData(propName);
                if (string.IsNullOrEmpty(data)) return "";
                result = result.Replace(match.Groups[0].Value, resource.GetCustomData(propName));
            }

            return result;
        }

        public string UserString
        {
            get { return Name; }
        }
    }
}
