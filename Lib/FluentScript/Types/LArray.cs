
using System.Collections;
using System.Collections.Generic;
using ComLib.Lang.Core;


namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a array
    /// </summary>
    public class LArray : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public LArray(IList val)
        {
            this.Value = val;
            this.Type = LTypes.Array;
        }


        /// <summary>
        /// The raw type value.
        /// </summary>
        public IList Value;


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
    /// Array datatype
    /// </summary>
    public class LArrayType : LObjectType
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LArrayType()
        {
            this.Name = "list";
            this.FullName = "sys.list";
            this.TypeVal = TypeConstants.Array;
            this.IsSystemType = true;
            // List<object>
        }


        /// <summary>
        /// Sets up the matrix of possible conversions from one type to another type.
        /// </summary>
        public override void SetupConversionMatrix()
        {
            this.SetDefaultConversionMatrix(TypeConversionMode.NotSupported);
            this.AddConversionTo(TypeConstants.Array,   TypeConversionMode.SameType);
        }
    }
}
