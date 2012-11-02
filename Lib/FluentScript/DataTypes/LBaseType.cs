using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Base class for datatypes.
    /// </summary>
    public abstract class LBaseType
    {
        /// <summary>
        /// Context of the script
        /// </summary>
        protected Context _context;


        /// <summary>
        /// Name of the variable
        /// </summary>
        protected string _varName;


        /// <summary>
        /// Value of the type.
        /// </summary>
        protected object _value;


        /// <summary>
        /// Whether or not indexer support is available.
        /// </summary>
        protected bool _supportsIndexer;


        /// <summary>
        /// Whether or not indexer is supported
        /// </summary>
        public bool SupportsIndexer { get { return _supportsIndexer; } }


        /// <summary>
        /// Whether or not this type supports the supplied method
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        public abstract bool HasMethod(string methodname);


        /// <summary>
        /// whether or not this type support the supplied property
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public abstract bool HasProperty(string propName);


        /// <summary>
        /// Calls the method
        /// </summary>
        /// <param name="methodname"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract object ExecuteMethod(string methodname, object[] args);


        /// <summary>
        /// Support for get/set value by int indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual object this[int index]
        {
            get { return null; }
            set { }
        }
    }
}
