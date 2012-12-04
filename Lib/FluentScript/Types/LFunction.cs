using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a array
    /// </summary>
    public class LFunction : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="type">The function type</param>
        /// <param name="val">The object</param>
        public LFunction(LFunctionType type, object val)
        {
            this.Value = val;
            this.Type = type;
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
    /// function in script
    /// </summary>
    public class LFunctionType : LObjectType
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LFunctionType(string name)
        {
            this.Name = name;
            this.FullName = name;
            this.TypeVal = TypeConstants.Function;
            this.Parent = null;
        }


        /// <summary>
        /// The parent of this type.
        /// </summary>
        public LType Parent;


        /// <summary>
        /// Sets up the matrix of possible conversions from one type to another type.
        /// </summary>
        public override void SetupConversionMatrix()
        {
            this.SetDefaultConversionMatrix(TypeConversionMode.NotSupported);
        }
    }
}
