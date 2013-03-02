using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Parsing;
using ComLib.Lang.Types;
// </lang:using>

namespace ComLib.Lang.Plugins
{

    /* *************************************************************************
    <doc:example>
    // Date plugin allows date expressions in a friendly format like January 1 2001;
    // Following formats are supported.
    
    var date = January 1st 2012;
    var date = Jan
    date = jan 10
    date = Jan 10 2012
    date = Jan 10, 2012
    date = Jan 10th
    date = Jan 10th 2012
    date = Jan 10th, 2012
    date = January 10
    date = January 10, 2012
    date = January 10th
    date = January 10th, 2012
    date = January 10th 2012 at 9:20 am; 
    
    if today is before December 25th 2011 then
	    print Still have time to buy gifts
    </doc:example>
    ***************************************************************************/
    // <fs:plugin-autogenerate>
    /// <summary>
    /// Combinator for handling dates.
    /// </summary>
    public class DatePlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public DatePlugin()
        {
            this.StartTokens = new string[]
            { 
                "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec",             
                "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", 
            
                "january",  "february", "march",    "april",    
                "may",      "june",     "july",     "august",   
                "september","october",  "november", "december",
                "January",  "February", "March",    "April",    
                "May",      "June",     "July",     "August",   
                "September","October",  "November", "December",

                "$DateToken"
            };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get 
            { 
                return "@month( jan | January ) @day<number>{1,2} ( st | nd | rd | th )? ( ','? @year( <number>{4} ) )?  ( at <time>)?"; 
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
                    "var date = January 1st 2012",
                    "var date = Jan",
                    "date = jan 10",
                    "date = Jan 10 2012",
                    "date = Jan 10, 2012",
                    "date = Jan 10th",
                    "date = Jan 10th 2012",
                    "date = Jan 10th, 2012",
                    "date = January 10",
                    "date = January 10, 2012",
                    "date = January 10th",
                    "date = January 10th, 2012",
                    "date = January 10th 2012 at 9:20 am"
                };
            }
        }
        // </fs:plugin-autogenerate>

        private static Dictionary<string, int> _months;


        static DatePlugin()
        {
            _months = new Dictionary<string, int>();
            _months["jan"] = 1;
            _months["feb"] = 2;
            _months["mar"] = 3;
            _months["apr"] = 4;
            _months["may"] = 5;
            _months["jun"] = 6;
            _months["jul"] = 7;
            _months["aug"] = 8;
            _months["sep"] = 9;
            _months["oct"] = 10;
            _months["nov"] = 11;
            _months["dec"] = 12;
            _months["january"] = 1;
            _months["february"] = 2;
            _months["march"] = 3;
            _months["april"] = 4;
            _months["may"] = 5;
            _months["june"] = 6;
            _months["july"] = 7;
            _months["august"] = 8;
            _months["september"] = 9;
            _months["october"] = 10;
            _months["november"] = 11;
            _months["december"] = 12;
        }


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool CanHandle(Token token)
        {
            var n1 = _tokenIt.Peek();
            var isLiteralDate = token.Kind == TokenKind.LiteralDate;

            if ( isLiteralDate && n1.Token.Text != "at")
                return false;

            // 1. 1st token is definitely month name in long or short form. "oct" or "october".
            var monthNameOrAbbr = token.Text.ToLower();
            return _months.ContainsKey(monthNameOrAbbr); 
        }


        /// <summary>
        /// Parses the date expression.
        /// - Oct 10
        /// - Oct 10 2011
        /// - Oct 10, 2011
        /// - Oct 10th
        /// - Oct 10th 2011
        /// - Oct 10th, 2011
        /// - October 10
        /// - October 10, 2011
        /// - October 10th
        /// - October 10th, 2011
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var startToken = _tokenIt.NextToken;
            var date = ParseDate(this);
            var exp = Exprs.Const(new LDate(date), startToken);
            return exp;
        }


        /// <summary>
        /// Parses a date.
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static DateTime ParseDate(ILangParser parser)
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            double day = DateTime.Now.Day;
            var time = new TimeSpan(0, 0, 0);
            var tokenIt = parser.TokenIt;
            TokenData n = null;
            if (tokenIt.NextToken.Token.Kind == TokenKind.LiteralDate)
            {
                var parsedDate = (DateTime)tokenIt.NextToken.Token.Value;
                year = parsedDate.Year;
                month = parsedDate.Month;
                day = parsedDate.Day;
            }
            else
            {
                // 1. 1st token is definitely month name in long or short form. "oct" or "october".
                var monthNameOrAbbr = tokenIt.ExpectId(true, true);
                month = _months[monthNameOrAbbr.ToLower()];

                // 2. Check for "," after month.
                if (tokenIt.NextToken.Token == Tokens.Comma) tokenIt.Advance();

                // 3. 2nd token is the day 10.
                day = tokenIt.ExpectNumber(false);

                // 4. Check for "st nd rd th" as in 1st, 2nd, 3rd 4th for the day part.
                n = tokenIt.Peek();
                var text = n.Token.Text.ToLower();
                if (text == "st" || text == "nd" || text == "rd" || text == "th")
                    tokenIt.Advance();

                // 5. Check for another "," after day part.
                n = tokenIt.Peek();
                var n2 = tokenIt.Peek(2);

                // IMPORTANT: Make sure not to interpret the "," as feb 12, if the the "," is part of a comma in array/map.
                if (n.Token == Tokens.Comma && n2.Token.Type == TokenTypes.LiteralNumber)
                {
                    tokenIt.Advance();
                }

                // 6. Finally check for year
                n = tokenIt.Peek();
                if (n.Token.Type == TokenTypes.LiteralNumber)
                {
                    year = Convert.ToInt32(n.Token.Text);
                    tokenIt.Advance();
                }
            }
            // 8. Now check for time.
            n = tokenIt.Peek();
            if (n.Token.Text == "at")
                time = TimeExprPlugin.ParseTime(parser, true, true);
            else            
                tokenIt.Advance();

            var date = new DateTime(year, month, (int)day, time.Hours, time.Minutes, time.Seconds);
            return date;
        }
    }
}
