using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Helpers;
using ComLib.Lang.Parsing;
// </lang:using>

namespace ComLib.Lang.Plugins
{

    /* *************************************************************************
    <doc:example>	
    // Time plugin provides a convenient way to represent time in fluent syntax.
    
    var t = 12:30 pm;
    
    if t is 12:30 pm then
	    print it's time to go to lunch!
    </doc:example>
    ***************************************************************************/
    /*
    /// <summary>
    /// Combinator for handling dates. noon afternoon. evening, nite midnight
    /// </summary>
    public class Time2Plugin : LexPlugin
    {
        private static Dictionary<string, TimeSpan> _aliases;
        private int _endPos = -1;
        private TimeSpan _time = TimeSpan.MinValue;

        /// <summary>
        /// Initialize
        /// </summary>
        static Time2Plugin()
        {
            _aliases = new Dictionary<string, TimeSpan>();
            _aliases["noon"] = new TimeSpan(12, 0, 0);
            _aliases["Noon"] = new TimeSpan(12, 0, 0);
            _aliases["midnight"] = new TimeSpan(0, 0, 0);
            _aliases["Midnight"] = new TimeSpan(0, 0, 0);
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public Time2Plugin()
        {
           _tokens = new string[] { "$NumberToken", "Noon", "noon", "midnight", "Midnight" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "( ( noon | Noon | midnight | Midnight ) | ( <number> ( ':' <number> ){1,2} ( am | pm ) ) ";
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
                    "noon",
                    "8 pm",
                    "8:30 pm",
                    "8:30:500 pm"
                };
            }
        }


        /// <summary>
        /// Whether or not this uri plugin can handle the current token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            _time = TimeSpan.MinValue;
            _endPos = -1;
            // Case 1: noon, "afternoon, midnight
            if (_aliases.ContainsKey(current.Text))
            {
                _time = CloneTime(_aliases[current.Text]);
                return true;
            }
            var text = Lexer.State.CurrentChar + _lexer.PeekChars(10);

            // Case 2: No more text available / End Of File
            if (string.IsNullOrEmpty(text))
                return false;

            // 1. Check for am/pm ( required )
            var isAm = true;
            var ndxAmOrPm = text.IndexOf("am", StringComparison.InvariantCultureIgnoreCase);
            if (ndxAmOrPm == -1)
            {
                ndxAmOrPm = text.IndexOf("pm", StringComparison.InvariantCultureIgnoreCase);
                if(ndxAmOrPm > -1)
                    isAm = false;
            }

            // Case 3: No am/pm designator so not applicable
            if (ndxAmOrPm == -1)
                return false;

            // Case 4: 830pm
            if(text.IndexOf(":") < 0 )
            {
                text = current.Text + text.Substring(0, ndxAmOrPm + 2);
                _time = DateTimeTypeHelper.ParseTimeWithoutColons(current.Text, isAm);
                _endPos = _lexer.State.Pos + ndxAmOrPm + 1;
                return true;
            }
            // Note: the longest time string can only be 9 chars e.g. :30:45 am
            text = current.Text + text.Substring(0, ndxAmOrPm + 2);
            var result = DateTimeTypeHelper.ParseTime(text);
            
            // Was not a valid time string. maybe some other plguin can handle it.
            if(!result.Item2)
                return false;

            _endPos = _lexer.State.Pos + ndxAmOrPm + 1;
            _time = result.Item1;
            return true;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            var lastToken = _lexer.LastTokenData;
            // Case 1: noon, afternoon, midnight
            if(_endPos == -1)
            {
                var aliasToken = new Token(TokenKind.LiteralTime, TokenTypes.LiteralDate, lastToken.Token.Text, _time);
                lastToken.Token = aliasToken;
                _lexer.ParsedTokens.Add(lastToken);
                return new Token[] { aliasToken };
            }

            // Case 2: 8:30:45 pm
            var line = _lexer.State.Line;
            var pos = _lexer.State.LineCharPosition;
            var separator = _lexer.State.CurrentChar;
            var textToken = _lexer.ReadToPosition(_endPos);
            var timeText = lastToken.Token.Text + separator + textToken.Text;
            var timeToken = new Token(TokenKind.LiteralTime, TokenTypes.LiteralDate, timeText, _time);

            var t = new TokenData() { Token = timeToken, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(t);
            return new Token[] { timeToken };
        }


        private TimeSpan CloneTime(TimeSpan t)
        {
            return new TimeSpan(t.Days, t.Hours, t.Minutes, t.Seconds);
        }
    }
    */
}
