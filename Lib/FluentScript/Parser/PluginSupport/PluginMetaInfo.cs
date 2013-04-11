using System.Collections.Generic;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser.PluginSupport
{
    /// <summary>
    /// Holds documentation for plugins.
    /// </summary>
    public class PluginMetaInfo
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Source code url
        /// </summary>
        public string SourceUrl { get; set; }


        /// <summary>
        /// Short description of the plugin.
        /// </summary>
        public string ShortDesc { get; set; }


        /// <summary>
        /// Long description of the plugin
        /// </summary>
        public string Desc { get; set; }


        /// <summary>
        /// Comma separated list of tags to help search for the plugin.
        /// </summary>
        public string Tags { get; set; }


        /// <summary>
        /// Plugin-type either expression, statement, token, lex
        /// </summary>
        public string PluginType { get; set; }


        /// <summary>
        /// The precedence of the plugins which helps to check for plugins in a certain order.
        /// </summary>
        public int Precedence { get; set; }


        /// <summary>
        /// The starting tokens which will trigger this plugin.
        /// </summary>
        public string[] StartTokens { get; set; }

        
        /// <summary>
        /// The grammer for the plugin
        /// </summary>
        public string Grammer { get; set; }


        /// <summary>
        /// Examples of the plugin
        /// </summary>
        public string[] Examples { get; set; }


        /// <summary>
        /// The author of the plugin
        /// </summary>
        public string Author { get; set; }


        /// <summary>
        /// Whether or not the plugin is a system level plugin.
        /// </summary>
        public bool IsSystemLevel { get; set; }


        /// <summary>
        /// Various notes/remarks about the plugin ( e.g. usage, setup, dev notes, etc )
        /// </summary>
        public string[] Notes { get; set; }


        /// <summary>
        /// Examples of this plugin in a non-fluent approach in another language.
        /// </summary>
        public List<KeyValuePair<string, string>>[] AnalygousExamples { get; set; }
    }
}
