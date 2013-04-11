using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Aggregate plugin allows sum, min, max, avg, count aggregate functions to 
    // be applied to lists of objects.
    
    var numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9];
    var result = 0;
    
    // Example 1: Using format sum of <expression>
    result = sum of numbers;
    result = avg of numbers;
    result = min of numbers;
    result = max of numbers;
    result = count of numbers;
    
    // Example 2: Using format sum(<expression>)
    result = sum( numbers );
    result = avg( numbers );
    result = min( numbers );
    result = max( numbers );
    result = count( numbers );    
    </doc:example>
    ***************************************************************************/
    // <fs:plugin-autogenerate>
    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class AnyOfPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public AnyOfPlugin()
        {
            this.IsAutoMatched = true;
            this.StartTokens = new string[] { "any" };
        }



        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "any of ( <expr> ( , <expr> )* )"; }
        }


        /// <summary>
        /// Examples
        /// </summary>
        public override string[] Examples
        {
            get
            {
                return new string[] { "if 2 == anyof ( 3, 4, 1, 2 )" };
            }
        }
        // </fs:plugin-autogenerate>


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var next = _tokenIt.Peek().Token;
            if (string.Compare(next.Text, "of", StringComparison.InvariantCultureIgnoreCase) == 0)
                return true;
            return false;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // 1. move past "of"
            _tokenIt.Advance(2);

            var anyofExpr = new AnyOfExpr();
            anyofExpr.Ctx = this.Ctx;
            anyofExpr.ParamListExpressions = new List<Expr>();
            anyofExpr.ParamList = new List<object>();

            // 2. any of ( <expression>* )
            if (_tokenIt.NextToken.Token == Tokens.LeftParenthesis)
            {
                _parser.ParseParameters(anyofExpr, true, true, false);
            }
            else
            {
                _parser.ParseParameters(anyofExpr, false, true, true);
            }            
            return anyofExpr;
        }
    }
}