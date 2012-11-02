using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
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
    public class TimePlugin : ExprPlugin
    {
        private static Dictionary<string, TimeSpan> _aliases;

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
            var time = ParseTime(this, false, false);
            return new ConstantExpr(time);
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
}
