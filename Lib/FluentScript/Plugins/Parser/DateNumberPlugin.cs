using System;
using System.Collections.Generic;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Date number plugin allow you to specify dates in the form of numbers as 
    // samples below.
    // The separator between months/days/years can be "/", "-", "\"
    
    var date1 = 1/27/1978;
    var date2 = 4-20-1979 at 4:30pm;
    var date3 = 6\10\2012 at 7:45am;
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling date numbers such as 1/27/1978
    /// </summary>
    public class DateNumberPlugin : LexPlugin
    {
        private static Dictionary<string, string> _keywords = new Dictionary<string, string>();
        private int _endPos = -1;

        /// <summary>
        /// Initialize
        /// </summary>
        public DateNumberPlugin()
        {
            _tokens = new string[] { "$NumberToken" };
            _canHandleToken = true;
        }


        /// <summary>
        /// Whether or not this uri plugin can handle the current token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool  CanHandle(Token current)
        {            
            _endPos = -1;
            
            var next = "";
            var pos = _lexer.State.Pos;

            // Check position.
            if (pos > _lexer.LAST_POSITION)
                return false;

            char n = _lexer.State.Text[pos];
            
            // Check that the next char is date part separator as in 3/10/2012 or 3-10-2012
            if (n != '-' && n != '/' && n != '\\')
                return false;

            while (pos <= _lexer.LAST_POSITION)
            {
                n = _lexer.State.Text[pos];
                if (Char.IsDigit(n))
                {
                    next += n;                    
                }
                else if (n == '-' || n == '/' || n == '\\')
                {
                    next += '/';
                }
                else
                    break;

                pos++;                
            }

            // No need to try parse the text if next lenght is < 5
            if (next.Length < 5) return false;

            var result = DateTime.MinValue;
            var combinedWord = current.Text + next;
            if (DateTime.TryParse(combinedWord, out result))
            {
                _endPos = pos - 1;
                return true;
            }
            return false;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "<number> ( '/' | '-' | '\' ) <number> ( '/' | '-' | '\' ) <number>"; }
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
                    "var active = 1/27/1978",
                    "var active = 4-20-1979",
                    @"var lookup = 6\10\2012"
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            // 04/20/1979
            var lastToken = _lexer.LastTokenData;
            var line = _lexer.State.Line;
            var pos = _lexer.State.LineCharPosition;
            var separator = _lexer.State.CurrentChar;
            var textToken = _lexer.ReadToPosition(_endPos);
            var dateText = lastToken.Token.Text + separator + textToken.Text;
            dateText = dateText.Replace("-", "/");
            dateText = dateText.Replace("\\", "/");
            var dateToken = TokenBuilder.ToLiteralDate(dateText);
            var t = new TokenData() { Token = dateToken, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(t);
            return new Token[] { dateToken };
        }
    }
}
