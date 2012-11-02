using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Boolean datatype.
    /// </summary>
    public class LBool : LObject
    {   
        /// <summary>
        /// Get boolean value.
        /// </summary>
        /// <returns></returns>
        public bool ToBool()
        {
            return (bool)_value;
        }
    }
}
