using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketResource : Value
    {
        public int ResourceTypeId { get; set; }
        public int ResourceId { get; set; }
        public int AccountId { get; set; }
        public string ResourceName { get; set; }
        public string ResourceCustomData { get; set; }

        public string GetCustomData(string fieldName)
        {
            if (string.IsNullOrEmpty(ResourceCustomData)) return "";
            var pattern = string.Format("\"Name\":\"{0}\",\"Value\":\"([^\"]+)\"", fieldName);
            return Regex.IsMatch(ResourceCustomData, pattern)
                ? Regex.Match(ResourceCustomData, pattern).Groups[1].Value : "";
        }

        public string GetCustomDataFormat(string fieldName, string format)
        {
            var result = GetCustomData(fieldName.Trim());
            return !string.IsNullOrEmpty(result) ? string.Format(format, result) : "";
        }

        public decimal GetCustomDataAsDecimal(string fieldName)
        {
            decimal result;
            decimal.TryParse(GetCustomData(fieldName), out result);
            return result;
        }
    }
}
