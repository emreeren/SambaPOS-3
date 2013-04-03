
using System.IO;
using ComLib.Lang.Core;

namespace ComLib.Lang.Runtime.Switches
{
    public class ExecuteSwitch : Switch
    {
        private string _filepath;


        public ExecuteSwitch(string filepath)
        {
            _filepath = filepath;
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        public override object DoExecute(Interpreter i)
        {
            i.ExecuteFile(_filepath);
            return null;
        }
    }
}
