using System.Collections.Generic;

namespace Fluentscript.Lib.Parser.MetaPlugins
{
    public class MetaPluginConstants
    {
        public static List<string> Plugins = new List<string>();

        public static void Init()
        {
            Plugins.Add("andor");
            Plugins.Add("bool");
            Plugins.Add("compare");
            Plugins.Add("set");
            Plugins.Add("step");
        }
    }
}
