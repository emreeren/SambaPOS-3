using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Suffix plugin enables the use of functions as postfix operators on constants.
    
    // create hours with timespan object using number of hours supplied
    function hours( num )
    {
         return new TimeSpan( 0, num, 0 , 0 )
    }

    // create minutes with timespan object using number of minutes supplied
    function minutes( num )
    {
        return new TimeSpan(0, 0, num, 0 )
    }

    // timespan objects can be added together
    var time = 3 hours + 20 minutes;
     
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class SuffixPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public SuffixPlugin()
        {
            this.StartTokens = new string[] { "$Suffix" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "<literal> <identifier>";
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
                    "3 shares",
                    "20 products"
                };
            }
        }


        /// <summary>
        /// Whether or not this plugin can handle current token(s).
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var t = _tokenIt.Peek(1, false);
            if (_parser.Context.Symbols.IsFunc(t.Token.Text))
                return true;
            return false;
        }


        /// <summary>
        /// Sorts expression
        /// </summary>
        /// <returns></returns>
        public override Expr Parse(object context)
        {
            var constExp = context as ConstantExpr;            
            var c = _tokenIt.NextToken;
            var t = _tokenIt.Advance();
            _parser.SetupContext(constExp, c);

            // Get the function symbol.
            var paramListExpressions = new List<Expr>();
            paramListExpressions.Add(constExp);
            var nameExp = Exprs.Ident(t.Token.Text, t);
            var expr = Exprs.FunctionCall(nameExp, paramListExpressions, t);

            // Move the postfix token.
            _tokenIt.Advance();
            return expr;
        }
    }
}