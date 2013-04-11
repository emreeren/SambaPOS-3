using System.Collections.Generic;
using Fluentscript.Lib.Parser;

namespace Fluentscript.Lib.Runtime.Switches
{
    public class ExecuteFilesSwitch : Switch
    {
        private List<string> _files;


        public ExecuteFilesSwitch(List<string> files)
        {
            _files = files;
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        public override object DoExecute(Interpreter i)
        {
            foreach (var file in _files)
            {
                i.AppendExecuteFile(file);
            }
            return null;
        }
    }
}
