using System.IO;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;

namespace Fluentscript.Lib.Runtime.Switches
{
    public class NodesSwitch : Switch
    {
        private string _filepath;
        private string _outpath;


        public NodesSwitch(string filepath, string outpath)
        {
            _filepath = filepath;
            _outpath = outpath;
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        public override object DoExecute(Interpreter i)
        {
            var statements = i.ToStatements(_filepath, true);
            using (var writer = new StreamWriter(_outpath))
            {
                foreach (Expr stmt in statements)
                {
                    writer.Write(stmt.AsString());
                }
                writer.Flush();
            }
            return null;
        }
    }
}
