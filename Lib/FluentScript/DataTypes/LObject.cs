using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Base type for all data types.
    /// </summary>
    public class LObject
    {
        /// <summary>
        /// Object value.
        /// </summary>
        protected object _value;


        /// <summary>
        /// The datatype.
        /// </summary>
        public Type DataType;


        /// <summary>
        /// Value of the type.
        /// </summary>
        public virtual object Value
        {
            get { return _value; }
        }
    }
}
