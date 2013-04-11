using System.Globalization;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser;

namespace Fluentscript.Lib.Runtime.Switches
{
    /// <summary>
    /// Class to handle the command line switch parameter for displaying plugins.
    /// </summary>
    public class ListPluginsSwitch : Switch
    {

        /// <summary>
        /// Prints all the meta plugins loaded.
        /// </summary>
        public override object DoExecute(Interpreter i)
        {
            var printer = new Printer();
            printer.WriteHeader("Meta plugins ");
            printer.WriteKeyValue(true, "Folder: ", false, i.Settings.PluginsFolder);
            printer.WriteLines(2);

            i.Context.PluginsMeta.EachPlugin(plugin =>
            {
                printer.WriteKeyValue(true, "Name: ", true, plugin.Name);
                printer.WriteKeyValue(true, "Desc: ", false, plugin.Desc);
                printer.WriteKeyValue(true, "Docs: ", false, plugin.Doc);
                printer.WriteKeyValue(true, "Type: ", false, plugin.PluginType);
                printer.WriteKeyValue(true, "IsOn: ", true, plugin.IsEnabled.ToString().ToLower());
                printer.WriteKeyValue(true, "Gram: ", true, plugin.GetFullGrammar());
                printer.WriteKeyValue(true, "Examples: ", false, string.Empty);
                for (var ndx = 0; ndx < plugin.Examples.Length; ndx++)
                {
                    var count = (ndx + 1).ToString(CultureInfo.InvariantCulture);
                    printer.WriteLine(count + ". " + plugin.Examples[ndx]);
                }
                printer.WriteLines(3);
            });
            return null;
        }
    }
}
