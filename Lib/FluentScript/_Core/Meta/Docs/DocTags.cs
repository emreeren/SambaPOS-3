using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang.Docs
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
            Args = new List<Arg>();
            Examples = new List<Example>();
        }


        /// <summary>
        /// The summary of the function/method.
        /// </summary>
        public string Summary { get; set; }


        /// <summary>
        /// List of all the argument doc tags.
        /// </summary>
        public List<Arg> Args { get; set; }


        /// <summary>
        /// List of all the examples of the function.
        /// </summary>        
        public List<Example> Examples { get; set; }
    }
}
