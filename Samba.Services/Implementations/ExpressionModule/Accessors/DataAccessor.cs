using System;
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
                return Model.GetType().GetProperty(fieldName).GetValue(Model, null);
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