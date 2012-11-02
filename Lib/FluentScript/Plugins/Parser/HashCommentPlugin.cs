using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
{

    /* *************************************************************************
    <doc:example>	
    // Allows using # for single line comments instead of //
    
    #  Single line comment 
    
    // Also single line comment
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling boolean values in differnt formats (yes, Yes, no, No, off Off, on On).
    /// </summary>
    public class HashCommentPlugin : LexPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public HashCommentPlugin()
        {
            _tokens = new string[] { "#" };
            _canHandleToken = true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "# . <newline>";
            }
        }


        /// <summary>
        /// Examples
        /// </summary>
        public override string[] Examples
        {
            get
            {
                return new string[]
                {
                    "# comment on one line",
                    "#single line comment same as using //"
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            // http https ftp ftps www 
            var takeoverToken = _lexer.LastTokenData;
            var line = _lexer.LineNumber;
            var pos = _lexer.LineCharPos;
            var n = _lexer.ReadChar();
            var token = _lexer.ReadLineRaw(false);
            token = ComLib.Lang.Tokens.ToComment(false, token.Text);
            var t = new TokenData() { Token = token, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(t);
            return new Token[] { token };
        }
    }
}
