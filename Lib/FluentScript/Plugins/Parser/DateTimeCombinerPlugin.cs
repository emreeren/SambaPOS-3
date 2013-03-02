using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Parsing;
// </lang:using>

namespace ComLib.Lang.Plugins
{

    /* *************************************************************************
    <doc:example>	    
    // Compare plugin allows word aliases for the comparison operators. See list below
    // 
    // ALIAS:                FOR:
    // "less than",          "<" 
    // "before",             "<" 
    // "below",              "<" 
    // "is below",           "<" 
    // "is before",          "<"
    // "more than",          ">" 
    // "after",              ">" 
    // "above",              ">" 
    // "is after",           ">" 
    // "is above",           ">" 
    // "less than equal",    "<="
    // "less than equal to", "<="
    // "more than equal",    ">="
    // "more than equal to", ">="
    // "is",                 "=="
    // "is equal",           "=="
    // "is equal to",        "=="
    // "equals",             "=="
    // "equal to",           "=="
    // "not",                "!="
    // "not equal",          "!="
    // "not equal to",       "!="
    // "is not",             "!="
    // "is not equal to",    "!=" 
    
    // Example 1: Using <
    if a less than b then
    if a before b    then 
    if a below  b    then
    if a is before b then
    if a is below b  then
    
    // Example 2: Using <=
    if less than equal then
    if less than equal to then
    
    // Example 2: Using >
    if a more than b then
    if a after b     then 
    if a above b     then
    if a is after b  then
    if a is above b  then    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class DateTimeCombinerPlugin : TokenReplacePlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public DateTimeCombinerPlugin()
        {
            this._tokens = new string[] { "$DateToken" };            
        }


        /// <summary>
        /// Whether or not this can handle the token supplied.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isCurrent"></param>
        /// <returns></returns>
        public override bool CanHandle(Token token, bool isCurrent)
        {
            var n1 = _tokenIt.Peek(1);
            if (n1.Token.Text != "at") 
                return false;

            var n2 = _tokenIt.Peek(2);
            if (n2.Token.Kind == TokenKind.LiteralTime)
                return true;

            return false;
        }


        /// <summary>
        /// Parses the tokens.
        /// </summary>
        /// <param name="advanceFirst"></param>
        /// <param name="advanceCount"></param>
        /// <returns></returns>
        public override Token Parse(bool advanceFirst, int advanceCount)
        {
            // 1. Get the date
            var dateToken = _tokenIt.NextToken;
            
            // 2. Expect "at"
            _tokenIt.Advance();
            _tokenIt.ExpectIdText("at");
            
            // 3. Get the time
            var timeToken = _tokenIt.NextToken;

            var date = (DateTime)dateToken.Token.Value;
            var time = (TimeSpan)timeToken.Token.Value;
            var result = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, time.Milliseconds, DateTimeKind.Local);

            var text = dateToken.Token.Text + " " + timeToken.Token.Text;
            var resultToken = new Token(TokenKind.LiteralTime, TokenTypes.LiteralDate, text, result);
            return resultToken;
        }
    }
}
