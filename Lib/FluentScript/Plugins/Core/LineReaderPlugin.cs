using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Core
{
    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class LineReaderPlugin : LexPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LineReaderPlugin()
        {
            _canHandleToken = true;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            return ParseLine(false);
        }


        /// <summary>
        /// Parse the entire line.
        /// </summary>
        /// <param name="includeNewLine"></param>
        /// <returns></returns>
        protected Token[] ParseLine(bool includeNewLine)
        {
            // print no quotes needed!
            var takeoverToken = _lexer.LastTokenData;
            int line = _lexer.State.Line;
            int pos  = _lexer.State.LineCharPosition;

            // This stops on the last char before the newline.
            // So move forward one.
            var lineToken = _lexer.ReadLine(false);
            var t = new TokenData() { Token = lineToken, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(takeoverToken);
            _lexer.ParsedTokens.Add(t);
            return new Token[] { takeoverToken.Token, lineToken };
        }
    }
}
