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
    public class ComparePlugin : TokenReplacePlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public ComparePlugin()
        {
            _tokens = new string[] 
            { 
                "less", "before", "below", "more", "after", "above", 
                "is", "equals", "not", "equal"
            };

            var replacements = new string[,]
            {
                { "less than",              "<" },                
                { "before",                 "<" },
                { "below",                  "<" },
                { "is below",               "<" },
                { "is before",              "<" },
                { "is less than",           "<" },
                
                { "more than",              ">" },
                { "after",                  ">" },
                { "above",                  ">" },
                { "is after",               ">" },
                { "is above",               ">" },
                { "is more than",           ">" },
                
                { "less than equal",        "<="},
                { "less than equal to",     "<="},
                { "is less than equal to",  "<="},
                
                { "more than equal",        ">="},
                { "more than equal to",     ">="},
                { "is more than equal to",  ">="},
                
                { "is",                     "=="},
                { "is equal",               "=="},
                { "is equal to",            "=="},
                { "equals",                 "=="},
                { "equal to",               "=="},
                
                { "not",                    "!="},
                { "not equal",              "!="},
                { "not equal to",           "!="},
                { "is not",                 "!="},
                { "is not equal to",        "!="},
            };
            base.Init(replacements, 5);
        }
    }
}
