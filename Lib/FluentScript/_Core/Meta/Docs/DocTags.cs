using System.Collections.Generic;
using Fluentscript.Lib._Core.Meta.Types;

namespace Fluentscript.Lib._Core.Meta.Docs
{
    /// <summary>
    /// A collection of all the doc tags for a function.
    /// </summary>
    public class DocTags
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public DocTags()
        {
            Args = new List<ArgAttribute>();
            Examples = new List<Example>();
            CustomTags = new List<CustomTag>();
        }


        /// <summary>
        /// The summary of the function/method.
        /// </summary>
        public string Summary { get; set; }


        /// <summary>
        /// List of all the argument doc tags.
        /// </summary>
        public List<ArgAttribute> Args { get; set; }


        /// <summary>
        /// List of all the examples of the function.
        /// </summary>        
        public List<Example> Examples { get; set; }


        /// <summary>
        /// Custom tags
        /// </summary>
        public List<CustomTag> CustomTags { get; set; } 
    }
}
