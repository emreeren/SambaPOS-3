using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Entities
{
    public class EntityType : EntityClass, IOrderable
    {
        public int SortOrder { get; set; }
        public string EntityName { get; set; }
        public int AccountTypeId { get; set; }
        public int WarehouseTypeId { get; set; }
        public string AccountNameTemplate { get; set; }

        private readonly IList<EntityCustomField> _entityCustomFields;
        public virtual IList<EntityCustomField> EntityCustomFields
        {
            get { return _entityCustomFields; }
        }

        public EntityType()
        {
            _entityCustomFields = new List<EntityCustomField>();
        }

        public EntityCustomField AddCustomField(string fieldName, int fieldType)
        {
            var result = new EntityCustomField { Name = fieldName, FieldType = fieldType };
            _entityCustomFields.Add(result);
            return result;
        }

        public IEnumerable<EntityCustomField> GetMatchingFields(Entity entity, string searchString)
        {
            if (!string.IsNullOrEmpty(entity.CustomData))
            {
                const string nameFormat = "\"Name\":\"{0}\"";
                var data = entity.CustomData.Split(new[] { "}," }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => Regex.Match(x, "\"Value\":\"([^\"]+)\"").Groups[1].Value.ToLower().Contains(searchString.ToLower()));
                return EntityCustomFields.Where(x => data.Any(y => y.ToLower().Contains(string.Format(nameFormat, x.Name).ToLower()) && y.ToLower().Contains(searchString.ToLower())));
            }
            return EntityCustomFields;
        }

        public string GenerateAccountName(Entity entity)
        {
            if (string.IsNullOrEmpty(entity.Name)) return "";

            var result = AccountNameTemplate;
            result = result.Replace("[Id]", entity.Id.ToString(CultureInfo.InvariantCulture));
            result = result.Replace("[Name]", entity.Name);
            while (Regex.IsMatch(result, "\\[([^\\]]+)\\]"))
            {
                var match = Regex.Match(result, "\\[([^\\]]+)\\]");
                var propName = match.Groups[1].Value;
                var data = entity.GetCustomData(propName);
                if (string.IsNullOrEmpty(data)) return "";
                result = result.Replace(match.Groups[0].Value, entity.GetCustomData(propName));
            }

            return result;
        }

        public string UserString
        {
            get { return Name; }
        }
    }
}
