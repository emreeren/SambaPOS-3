using System;
using System.Collections.Generic;

namespace Fluentscript.Lib._Core.Meta.Types
{
    /// <summary>
    /// Information about parameters to a function.
    /// </summary>
    public class ArgAttribute : Attribute
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        public ArgAttribute()
        {
            Examples = new List<string>();
        }


        /// <summary>
        /// The name of the argument
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Description of the Parameter
        /// </summary>
        public string Desc { get; set; }


        /// <summary>
        /// Datatype of the parameter
        /// </summary>
        public string Type { get; set; }


        /// <summary>
        /// Another alias for the parameter
        /// </summary>
        public string Alias { get; set; }


        /// <summary>
        /// The 0 based index position of this argument in the parameter list.
        /// </summary>
        public int Index { get; set; }


        /// <summary>
        /// Whether or not this argument is required or can be null.
        /// </summary>
        public bool Required { get; set; }


        /// <summary>
        /// Default value of argument.
        /// </summary>
        public object DefaultValue { get; set; }


        /// <summary>
        /// List of example values for the paramter.
        /// </summary>
        public List<string> Examples { get; set; }
    }
}
