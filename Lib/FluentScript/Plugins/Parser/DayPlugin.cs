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
    // Day plugin allows days of the week to be used as words. 
    // lowercase and uppercase days are supported:
    // 1. Monday - Sunday
    // 2. monday - sunday
    // 3. today, tomorrow, yesterday
    
    var day = Monday;
    var date = tomorrow at 3:30 pm;
    
    if tommorrow is Saturday then
	    print Thank god it's Friday
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class DayPlugin : ExprPlugin
    {
        private static Dictionary<string, DayOfWeek> _days;
        private static Dictionary<string, Func<DateTime>> _dayAliases;


        static DayPlugin()
        {
            _days = new Dictionary<string, DayOfWeek>();            
            _days["monday"]     = DayOfWeek.Monday;
            _days["tuesday"]    = DayOfWeek.Tuesday;
            _days["wednesday"]  = DayOfWeek.Wednesday;
            _days["thursday"]   = DayOfWeek.Thursday;
            _days["friday"]     = DayOfWeek.Friday;
            _days["saturday"]   = DayOfWeek.Saturday;
            _days["sunday"]     = DayOfWeek.Sunday;

            _dayAliases = new Dictionary<string, Func<DateTime>>();
            _dayAliases["today"]     = () => DateTime.Now;
            _dayAliases["yesterday"] = () => DateTime.Now.AddDays(-1);
            _dayAliases["tomorrow"] = () => DateTime.Now.AddDays(1);
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public DayPlugin()
        {
            this.StartTokens = new string[]
            { 
                "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday",
                "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday",

                "today", "tomorrow", "yesterday",
                "Today", "Tomorrow", "Yesterday"
            };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "( ( today | tomorrow | yesterday ) | ( monday | tuesday | wednesday ... ) ) ( at <time> )?";
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
                    "var day = Monday",
                    "var date = tomorrow at 3:30 pm"
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
            if (token.Kind != TokenKind.Ident) return false;
            string name = token.Text.ToLower();
            if (_days.ContainsKey(name)) return true;
            if (_dayAliases.ContainsKey(name)) return true;

            return false;
        }


        /// <summary>
        /// Parses the day expression.
        /// Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var startToken = _tokenIt.NextToken;
            var name = _tokenIt.ExpectId().ToLower();

            // 1. Day of week : "monday" or "Monday" etc.
            if (_days.ContainsKey(name))
                return Exprs.Const(new LDayOfWeek(_days[name]), startToken);

            // 2. DateTime ( today, yesterday, tommorrow )
            var dateTime = _dayAliases[name]();
            if (_tokenIt.NextToken.Token.Text != "at")
            {
                return Exprs.Const(new LDate(dateTime), startToken);
            }

            var time = TimeExprPlugin.ParseTime(_parser, true, true);
            dateTime = new DateTime(dateTime.Year, dateTime.Month, (int)dateTime.Day, time.Hours, time.Minutes, time.Seconds);
            return Exprs.Const(new LDate(dateTime), startToken);
        }
    }
}
