using Samba.Domain.Models.Entities;

namespace Samba.Services.Implementations.ExpressionModule.Accessors
{
    public static class EntityAccessor
    {
        private static Entity _model;
        public static Entity Model
        {
            get { return _model ?? (_model = Entity.Null); }
            set { _model = value; }
        }

        public static string Name { get { return Model.Name; } set { Model.Name = value; } }
        public static string CustomData { get { return Model.CustomData; } set { Model.CustomData = value; } }

        public static string GetCustomData(string fieldName)
        {
            return Model.GetCustomData(fieldName);
        }
    }
}