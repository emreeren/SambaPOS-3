using System.IO;
using System.Collections.Generic;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Runtime.Switches
{
    public class TokensSwitch : Switch
    {
        private string _filepath;
        private string _outpath;
        private bool _excludeNewLines;
        private bool _conciseMode;


        public TokensSwitch(string filepath, string outpath)
        {
            _filepath = filepath;
            _outpath = outpath;
            _excludeNewLines = true;
            _conciseMode = true;
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        public override object DoExecute(Interpreter i)
        {
            i.InitPlugins();
            var tokens = i.ToTokens(_filepath, true);
            using (var writer = new StreamWriter(_outpath))
            {
                foreach (TokenData tokendata in tokens)
                {
                    // Option 1: Do not display newlines in the token list
                    if (_excludeNewLines && tokendata.Token == Tokens.NewLine)
                        continue;

                    // Option 2: Just include line #, line char # and text.
                    if (_conciseMode)
                    {
                        // Create only if needed.
                        if (_paddings == null)
                            _paddings = new Dictionary<int, string>();
                        
                        var text = tokendata.Line.ToString() + " - " + tokendata.LineCharPos.ToString();
                        var lineinfo = this.Pad(text, 8);
                        var tokenText = tokendata.Token.Text;
                        if (tokendata.Token.Kind == TokenKind.LiteralString)
                            tokenText = "'" + tokenText + "'";
                        writer.WriteLine("{0} : {1}", lineinfo, tokenText);
                    }
                    else
                        writer.WriteLine(tokendata.ToString());
                }
                writer.Flush();
            }
            return null;
        }


        private string Pad(string text, int max)
        {
            if (text.Length == max) return text;
            var diff = max - text.Length;

            // Memoize
            if (_paddings != null && _paddings.ContainsKey(diff))
                return text + _paddings[diff];

            var padding = "";
            var count = diff;
            while (count > 0)
            {
                padding +=  ' ';
                count--;
            }
            if (_paddings != null)
                _paddings[diff] = padding;

            return text + padding;
        }


        private static Dictionary<int, string> _paddings;

    }
}
