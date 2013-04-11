using System;
using Fluentscript.Lib.Parser;

namespace Fluentscript.Lib.Runtime.Switches
{
    public class InteractiveSwitch : Switch
    {
        private string _filepath;


        public InteractiveSwitch(string filepath)
        {
            _filepath = filepath;
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        public override object DoExecute(Interpreter i)
        {
            i.InitPlugins();

            // 1. read line of code from console.
            var script = Console.ReadLine();
            script = script.Trim();
            if (string.Compare(script, "exit", StringComparison.InvariantCultureIgnoreCase) == 0)
                return string.Empty;

            i.Execute(script);

            // 2. Check success of line
            if (!i.Result.Success)
                return i.Result.Message;

            while (true)
            {
                // Now keep looping
                // 3. Read successive lines of code and append
                script = Console.ReadLine();

                // 4. Check for exit flag.
                if (string.Compare(script, "exit", StringComparison.InvariantCultureIgnoreCase) == 0
                    || string.Compare(script, "Exit", StringComparison.InvariantCultureIgnoreCase) == 0
                    || string.Compare(script, "EXIT", StringComparison.InvariantCultureIgnoreCase) == 0)
                    break;

                // 5. Only process if not empty
                if (!string.IsNullOrEmpty(script))
                {
                    i.AppendExecute(script);

                    // 6. if error break;
                    if (!i.Result.Success)
                        break;
                }
            }
            return null;
        }
    }
}
