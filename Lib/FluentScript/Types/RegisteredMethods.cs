using System.Collections.Generic;

namespace Fluentscript.Lib.Types
{

    /// <summary>
    /// Support the mapping of methods to dataypes.
    /// </summary>
    public class RegisteredMethods
    {        
        private IDictionary<LType, ITypeMethods> _typeToMethods = new Dictionary<LType, ITypeMethods>();


        /// <summary>
        /// Register methods on a specific type.
        /// </summary>
        /// <param name="type">The datatype for which the methods implementation are applicable</param>
        /// <param name="methods">The method implementations</param>
        public void Register(LType type, ITypeMethods methods)
        {
            _typeToMethods[type] = methods;
        }


        /// <summary>
        /// Registers methods on a specific type if no existing methods implementation are already
        /// registered for the type.
        /// </summary>
        /// <param name="type">The datatype for which the methods implementation are applicable</param>
        /// <param name="methods">The method implementations</param>
        public void RegisterIfNotPresent(LType type, ITypeMethods methods)
        {
            if (!_typeToMethods.ContainsKey(type))
            {
                _typeToMethods[type] = methods;
                methods.OnRegistered();
            }
        }


        /// <summary>
        /// Whether or not there are methods for the supplied type.
        /// </summary>
        /// <param name="type"></param>
        public bool Contains(LType type)
        {
            return _typeToMethods.ContainsKey(type);
        }


        /// <summary>
        /// Get the methods implementation for the supplied type.
        /// </summary>
        /// <param name="type"></param>
        public ITypeMethods Get(LType type)
        {
            return _typeToMethods[type];
        }
    }
}
