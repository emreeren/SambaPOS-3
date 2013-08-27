using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Automation
{
    [DataContract]
    public class RuleConstraintValue
    {
        [DataMember(Name = "N")]
        public string Name { get; set; }
        [DataMember(Name = "L")]
        public string Left { get; set; }
        [DataMember(Name = "O")]
        public string Operation { get; set; }
        [DataMember(Name = "R")]
        public string Right { get; set; }

        public bool Satisfies(object dataObject)
        {
            var left = GetData(dataObject, Left);
            var type = GetDataType(dataObject, Left);
            return Utility.IsNumericType(type)
                       ? CompareNumeric(left, Right, Operation)
                       : Compare(left.ToString(), Right, Operation);
        }

        private bool Compare(string left, string right, string operation)
        {
            switch (operation)
            {
                case Operations.IsNull: return string.IsNullOrEmpty(left);
                case Operations.Contains: return left.Contains(right);
                case Operations.Starts: return left.StartsWith(right);
                case Operations.Ends: return left.EndsWith(right);
                case Operations.Matches: return Regex.IsMatch(left, right);
                case Operations.NotMatches: return !Regex.IsMatch(left, right);
                case Operations.NotEquals: return left != right;
                default: return left == right;
            }
        }

        private bool CompareNumeric(object left, object right, string operation)
        {
            decimal n1;
            decimal.TryParse(left.ToString(), out n1);
            decimal n2;
            decimal.TryParse(right.ToString(), out n2);

            switch (operation)
            {
                case Operations.Greater: return n1 < n2;
                case Operations.Less: return n1 > n2;
                case Operations.NotEquals: return n1 != n2;
                default: return n1 == n2;
            }
        }

        public bool ContainsData(object dataObject, string propertyName)
        {
            return ((IDictionary<string, object>)dataObject).ContainsKey(propertyName);
        }

        public object GetData(object dataObject, string propertyName)
        {
            if (ContainsData(dataObject, propertyName))
            {
                return ((IDictionary<string, object>)dataObject)[propertyName];
            }
            return propertyName;
        }

        public Type GetDataType(object dataObject, string propertyName)
        {
            if (ContainsData(dataObject, propertyName))
            {
                return ((IDictionary<string, object>)dataObject)[propertyName].GetType();
            }
            return typeof(string);
        }
    }
}