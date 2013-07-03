using System;
using System.Collections.Generic;
using Samba.Domain.Models.Entities;

namespace Samba.Services.Implementations.ExpressionModule.Accessors
{
    public static class DataAccessor
    {
        private static object _model;
        public static object Model
        {
            get { return _model ?? (_model = Entity.Null); }
            set { _model = value; }
        }

        public static object Get(string fieldName)
        {
            try
            {
                if (((IDictionary<string, object>)Model).ContainsKey(fieldName))
                    return ((IDictionary<string, object>)Model)[fieldName];
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Contains(string fieldName, string value)
        {
            var val = Get(fieldName);
            return val != null && val.ToString().Contains(value);
        }
    }
}