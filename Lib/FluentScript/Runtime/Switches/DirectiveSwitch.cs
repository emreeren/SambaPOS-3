using Fluentscript.Lib.Parser;

namespace Fluentscript.Lib.Runtime.Switches
{
    public class DirectiveSwitch : Switch
    {
        private string _directives;


        public DirectiveSwitch(string directives)
        {
            _directives = directives;
            this.OutputResult = false;
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        public override object DoExecute(Interpreter i)
        {
            i.Context.Directives.RegisterDelimited(_directives);
            return null;
        }
    }
}
