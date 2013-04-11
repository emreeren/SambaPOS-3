using System;
using System.Collections.Generic;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser.Integration
{
    /// <summary>
    /// Helper class for calling functions
    /// </summary>
    public class RegisteredTypes
    {
        class RegisteredType
        {
            /// <summary>
            /// Whether or not this is a basic type ( double, string, date, bool )
            /// </summary>
            public bool IsBasicType;


            /// <summary>
            /// Full name of the type.
            /// </summary>
            public string FullName;
            
            
            /// <summary>
            /// Short name of the type.
            /// </summary>
            public string Name;


            /// <summary>
            /// DataType
            /// </summary>
            public Type DataType;


            /// <summary>
            /// Instance creator.
            /// </summary>
            public Func<object> Creator;
        }



        private Dictionary<string, RegisteredType> _types;


        /// <summary>
        /// Initialize
        /// </summary>
        public RegisteredTypes()
        {
            _types = new Dictionary<string, RegisteredType>();   
        }


        /// <summary>
        /// Register a custom type into the interpreter( for calling c# from the language ).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="creator"></param>
        public void Register(Type type, Func<object> creator)
        {
            bool isBasicType = false;
            if( type == typeof(DateTime) )
            {
                isBasicType = true;
                creator = () => new DateTime(DateTime.Now.Ticks);
            }

            var registeredType = new RegisteredType() 
            { 
                IsBasicType = isBasicType, DataType = type, Creator = creator,
                Name = type.Name, FullName = type.FullName
            };
            _types[type.Name] = registeredType;
            _types[type.FullName] = registeredType;
        }


        /// <summary>
        /// Whether or not the typename supplied exists in the registered types
        /// </summary>
        /// <param name="nameOrFullName"></param>
        /// <returns></returns>
        public bool Contains(string nameOrFullName)
        {
            return _types.ContainsKey(nameOrFullName);
        }


        /// <summary>
        /// Whether or not the typename supplied exists in the registered types
        /// </summary>
        /// <param name="nameOrFullName"></param>
        /// <returns></returns>
        public Type Get(string nameOrFullName)
        {
            return _types[nameOrFullName].DataType;
        }


        /// <summary>
        /// Create new instance of typename.
        /// </summary>
        /// <param name="nameOrFullName"></param>
        /// <param name="args">The arguments for a constructor</param>
        /// <returns></returns>
        public object Create(string nameOrFullName, object[] args = null )
        {
            var registeredType = _types[nameOrFullName];
            object customType = args == null
                              ? Activator.CreateInstance(registeredType.DataType)
                              : Activator.CreateInstance(registeredType.DataType, args);
            return customType;
        }
    }
}
