using System;

using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a bool value.
    /// </summary>
    public class LClass : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val">The type of the external c# class.</param>
        public LClass(object val)
        {
            this.Value = val;
            this.Type = new LClassType(val.GetType());
        }


        /// <summary>
        /// The raw type value.
        /// </summary>
        public object Value;


        /// <summary>
        /// Gets the value of this object.
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return this.Value;
        }
    }


    /// <summary>
    /// Array type.
    /// </summary>
    public class LClassType : LObjectType
    {
        /// <summary>
        /// Used for now since fluentscript doesn't support classes.
        /// But this is used for using external c# classes in fluentscript.
        /// </summary>
        public Type DataType;


        /// <summary>
        /// Initialize.
        /// </summary>
        public LClassType(Type type)
        {
            // To be determined during parsing phase.
            this.Name = type.Name;
            this.FullName = type.FullName;
            this.TypeVal = TypeConstants.LClass;
            this.DataType = type;
        }
    }
}
