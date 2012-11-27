using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Types;
using ComLib.Lang.Parsing;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.Plugins
{

    /* *************************************************************************
    <doc:example>	
    // Exec plugin allows launching/execution of external programs.
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
    public class CustomFunctionPluginBase : ExprPlugin
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name = string.Empty;


        /// <summary>
        /// Metadata about the custom function.
        /// </summary>
        protected FunctionMetaData _funcMeta;


        /// <summary>
        /// Initialize
        /// </summary>
        public void Init(string name)
        {
            this.Name = name;
            this.StartTokens = new string[] { this.Name };
            this.IsStatement = true;
            this.IsEndOfStatementRequired = true;
            this.IsAutoMatched = true;
        }  


        /// <summary>
        /// Parses the day expression.
        /// Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        /// </summary>
        /// <returns></returns>
        public Expr ParseFunction(ParameterExpr expr)
        {
            _tokenIt.ExpectIdText(this.Name);
            var expectParens = _tokenIt.NextToken.Token == Tokens.LeftParenthesis;
            FluentHelper.ParseFuncParameters(expr.ParamListExpressions, _tokenIt, _parser, expectParens, true, _funcMeta);
            return expr;
        }
    }
}
