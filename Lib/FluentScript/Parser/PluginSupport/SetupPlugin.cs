using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
// </lang:using>

namespace ComLib.Lang.Parsing
{
    /// <summary>
    /// Setup plugin to configure the interpreter/context
    /// </summary>
    public class SetupPlugin : ISetupPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public SetupPlugin()
        {
            this.Id = "ComLib." + this.GetType().Name.Replace("Plugin", string.Empty);
        }


        /// <summary>
        /// Id of the setup plugin
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// Precedencce of this plugin in relation to other setup plguins.
        /// </summary>
        public int Precedence { get; set; }


        /// <summary>
        /// Executes a setup on the interpreter
        /// </summary>
        /// <param name="ctx"></param>
        public virtual void Setup(Context ctx)
        {
        }
    }
}
