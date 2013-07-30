using Fluentscript.Lib.Parser.Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser.PluginSupport
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
            this.Id = "Fluentscript.Lib." + this.GetType().Name.Replace("Plugin", string.Empty);
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
