using System;
using System.Collections.Generic;
using Fluentscript.Lib.Parser.Core;

namespace Fluentscript.Lib.Runtime.Bindings
{
    public class LanguageBinding
    {
        /// <summary>
        /// Context of the runtime.
        /// </summary>
        public Context Ctx { get; set; }


        /// <summary>
        /// Component name for this binding.
        /// </summary>
        public string ComponentName { get; set;  }


        /// <summary>
        /// The namespace of the binding
        /// </summary>
        public string Namespace { get; set; }


        /// <summary>
        /// Whether or not this binding supports functions
        /// </summary>
        public bool SupportsFunctions { get; set; }


        /// <summary>
        /// The exported publically available functions.
        /// </summary>
        public List<string> ExportedFunctions { get; set; }


        /// <summary>
        /// Can optionally set a naming convention.
        /// </summary>
        public string NamingConvention { get; set; }


        /// <summary>
        /// Executes a function in this language binding.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object ExecuteFunction(string name, object[] args)
        {
            // Naming convention ? default to camel case
            if(!string.IsNullOrEmpty(this.NamingConvention))
            {
                name = name[0].ToString().ToUpper() + name.Substring(1);
            }
            var method = this.GetType().GetMethod(name);
            if(method == null)
                throw new ArgumentException("Binding for " + this.ComponentName + " does not have function " + name);
            object result = method.Invoke(this, args);
            return result;
        }
    }
}
