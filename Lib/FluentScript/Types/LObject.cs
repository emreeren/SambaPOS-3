using System;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Types
{
    /// <summary>
    /// Used to store/wrap a value of either a basic type or instance of a class.
    /// </summary>
    public class LObject
    {
        /// <summary>
        /// The data type of this value.
        /// </summary>
        public LType Type;


        /// <summary>
        /// Gets the value of this object.
        /// </summary>
        /// <returns></returns>
        public virtual object GetValue()
        {
            return null;
        }

        //fix
        public object GetValue(Type type)
        {
            return Convert.ChangeType(GetValue(), type);
        }

        /// <summary>
        /// Clones this value.
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return this;
        }
    }



    /// <summary>
    /// LObjectType class that all types extend from.
    /// </summary>
    public class LObjectType : LType
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LObjectType()
        {
            this.Name = "object";
            this.FullName = "sys.object";
            this.TypeVal = TypeConstants.Any;
            this.IsSystemType = true;
        }


        ///// <summary>
        ///// Sets up the matrix of possible conversions from one type to another type.
        ///// </summary>
        //public override void SetupConversionMatrix()
        //{
        //    this.AddConversionTo(TypeConstants.Array,     TypeConversionMode.NotSupported);
        //    this.AddConversionTo(TypeConstants.Bool,      TypeConversionMode.NotSupported);
        //    this.AddConversionTo(TypeConstants.Date,      TypeConversionMode.NotSupported);
        //    this.AddConversionTo(TypeConstants.Map,       TypeConversionMode.NotSupported);
        //    this.AddConversionTo(TypeConstants.Number,    TypeConversionMode.NotSupported);
        //    this.AddConversionTo(TypeConstants.Null,      TypeConversionMode.Supported);
        //    this.AddConversionTo(TypeConstants.String,    TypeConversionMode.NotSupported);
        //    this.AddConversionTo(TypeConstants.Time,      TypeConversionMode.NotSupported);
        //}
    }

}
