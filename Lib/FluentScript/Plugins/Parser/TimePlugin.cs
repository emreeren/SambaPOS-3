using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Time plugin provides a convenient way to represent time in fluent syntax.
    
    var t = 12:30 pm;
    
    if t is 12:30 pm then
	    print it's time to go to lunch!
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling dates. noon afternoon. evening, nite midnight
    /// </summary>
    public class TimeExprPlugin : ExprPlugin
    {
        private static Dictionary<string, TimeSpan> _aliases;

        /// <summary>
        /// Initialize
        /// </summary>
        static TimeExprPlugin()
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
        public TimeExprPlugin()
        {
            this.StartTokens = new string[] { "$NumberToken", "Noon", "noon", "midnight", "Midnight" };
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
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool CanHandle(Token token)
        {
            if (_aliases.ContainsKey(token.Text.ToLower()))
                return true;

            // token has to be a literal token.
            if (!(token.Type == TokenTypes.LiteralNumber)) return false;

            var next = _tokenIt.Peek().Token;
            if ( next != Tokens.Colon && !( next.Text == "am" || next.Text == "pm" )) 
                return false;

            return true;
        }


        /// <summary>
        /// Parses the time expression.
        /// - 12pm
        /// - 12:30pm
        /// - 12:30 pm
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var startToken = _tokenIt.NextToken;
            var time = ParseTime(this, false, false);
            return Exprs.Const(new LTime(time), startToken);
        }

    
        /// <summary>
        /// Parses the time in format 12[:minutes:seconds] am|pm.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="advance"></param>
        /// <param name="expectAt"></param>
        /// <returns></returns>
        public static TimeSpan ParseTime(ILangParser parser, bool advance, bool expectAt)
        {
            double minutes = 0;
            double seconds = 0;
            double hours = DateTime.Now.Hour;

            var tokenIt = parser.TokenIt;
            if(advance) tokenIt.Advance();

            if (expectAt)
            {
                // 1. Expect 'at"
                tokenIt.ExpectIdText("at", true);
            }

            // Next token could be a Literal time token
            // 1. time is a basic datatype
            // 2. time could be represented as 9:30 am and could have been
            //    lexically parsed by a lex plugin.
            //    NOTE: This plugin(any all other plugins) should NOT
            //    know or depend on any other plugin.
            //    However, they can know about tokens / basic types.
            if (tokenIt.NextToken.Token.Kind == TokenKind.LiteralTime)
            {
                var timeVal = (TimeSpan)tokenIt.NextToken.Token.Value;
                tokenIt.Advance();
                return timeVal;
            }

            string tokenText = tokenIt.NextToken.Token.Text.ToLower();
            if (_aliases.ContainsKey(tokenText))
            {
                tokenIt.Advance();
                return _aliases[tokenText];
            }
                        
            // 3. Hour part
            hours = tokenIt.ExpectNumber(true);
            
            // 4. Time specified without colon: e.g. 1030 pm ?
            var next = tokenIt.NextToken.Token.Text;
            if ((next == "am" || next == "pm") && tokenText.Length > 2)
            {                
                var time = hours;
                // 130 - 930    am|pm
                if (time < 1000)
                {
                    hours = Convert.ToDouble(tokenText[0].ToString());
                    minutes = Convert.ToDouble(tokenText.Substring(1));
                }
                // 1030 - 1230  am|pm                
                else
                {
                    hours = Convert.ToDouble(tokenText.Substring(0, 2));
                    minutes = Convert.ToDouble(tokenText.Substring(2));
                }

            }
            // 5. Time specified with ":" 10:30 pm
            else if (next == ":")
            {
                tokenIt.Advance();

                // 6. Minutes part.
                minutes = tokenIt.ExpectNumber(true);

                if (tokenIt.NextToken.Token == Tokens.Colon)
                {
                    tokenIt.Advance();
                    seconds = tokenIt.ExpectNumber(true);
                }
            }

            var text = tokenIt.NextToken.Token.Text;

            if (text != "am" && text != "pm")
                throw tokenIt.BuildSyntaxExpectedException("am/pm");

            if (text == "pm" && hours >= 1 && hours <= 11)
                hours += 12;

            tokenIt.Advance();
            return new TimeSpan((int)hours, (int)minutes, (int)seconds);
        }
    }


    /* *************************************************************************
    <doc:example>	
    // Time plugin provides a convenient way to represent time in fluent syntax.
    
    var t = 12:30 pm;
    
    if t is 12:30 pm then
	    print it's time to go to lunch!
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling dates. noon afternoon. evening, nite midnight
    /// </summary>
    public class TimePlugin : LexPlugin
    {
        private static Dictionary<string, TimeSpan> _aliases;
        private int _endPos = -1;
        private TimeSpan _time = TimeSpan.MinValue;

        /// <summary>
        /// Initialize
        /// </summary>
        static TimePlugin()
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
        public TimePlugin()
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

            // Check 1: Make sure it's a number.
            if (current.Kind != TokenKind.LiteralNumber)
                return false;
            
            var nextToken = _lexer.PeekToken();
            var nextTokenText = nextToken.Token.Text.ToLower();

            // Check 2: End token?
            if (nextToken.Token == Tokens.EndToken)
                return false;
                        
            // Check 3: Time format is 9:30 am or 930 am
            // So if next token is not ':' then it has to be "am" or "pm"
            if (nextTokenText != Tokens.Colon.Text
                && nextTokenText != "am" && nextTokenText != "pm")
                return false;

            var text = Lexer.State.CurrentChar + _lexer.Scanner.PeekMaxChars(10);
            // 1. Check for am/pm ( required )
            var isAm = true;
            var ndxAmOrPm = text.IndexOf("am", StringComparison.InvariantCultureIgnoreCase);
            if (ndxAmOrPm == -1)
            {
                ndxAmOrPm = text.IndexOf("pm", StringComparison.InvariantCultureIgnoreCase);
                if (ndxAmOrPm > -1)
                    isAm = false;
            }

            // Case 3: No am/pm designator so not applicable
            if (ndxAmOrPm == -1)
                return false;

            // Case 4: 830pm
            if (text.IndexOf(":") < 0)
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
            if (!result.Item2)
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
            if (_endPos == -1)
            {
                var aliasToken = new Token(TokenKind.LiteralTime, TokenTypes.LiteralTime, lastToken.Token.Text, _time);
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
            var timeToken = TokenBuilder.ToLiteralTime(timeText, _time);

            var t = new TokenData() { Token = timeToken, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(t);
            return new Token[] { timeToken };
        }


        private TimeSpan CloneTime(TimeSpan t)
        {
            return new TimeSpan(t.Days, t.Hours, t.Minutes, t.Seconds);
        }
    }
}
