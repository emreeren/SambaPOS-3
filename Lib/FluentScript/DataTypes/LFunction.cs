using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// function in script
    /// </summary>
    class LFunction : LBaseType
    {
        /// <summary>
        /// Whether or not the function has the method supplied
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        public override bool HasMethod(string methodname)
        {
            return false;
        }


        /// <summary>
        /// Whether or not this function has the property supplied.
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public override bool HasProperty(string propName)
        {
            return false;
        }


        /// <summary>
        /// Executes the method.
        /// </summary>
        /// <param name="methodname"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object ExecuteMethod(string methodname, object[] args)
        {
            return null;
        }
    }
}
