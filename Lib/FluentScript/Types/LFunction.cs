using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
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
        }


        /// <summary>
        /// Sets up the matrix of possible conversions from one type to another type.
        /// </summary>
        public override void SetupConversionMatrix()
        {
            this.SetDefaultConversionMatrix(TypeConversionMode.NotSupported);
        }
    }
}
