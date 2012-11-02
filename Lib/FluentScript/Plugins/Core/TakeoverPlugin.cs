using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
{
    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class TakeoverPlugin : LexPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public TakeoverPlugin()
        {
            _canHandleToken = true;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            // print no quotes needed!
            var takeoverToken = _lexer.LastTokenData;
            int line = _lexer.LineNumber;
            int pos  = _lexer.LineCharPos;
            var lineToken = _lexer.ReadLine(false);
            var t = new TokenData() { Token = lineToken, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(takeoverToken);
            _lexer.ParsedTokens.Add(t);
            return new Token[] { takeoverToken.Token, lineToken };
        }
    }
}
