using Fluentscript.Lib.Parser;

namespace Fluentscript.Lib.Runtime.Switches
{
    public class LintSwitch : Switch
    {
        private string _filepath;


        public LintSwitch(string filepath)
        {
            _filepath = filepath;
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        public override object DoExecute(Interpreter i)
        {
            i.LintFile(_filepath);
            return null;
        }
    }
}
