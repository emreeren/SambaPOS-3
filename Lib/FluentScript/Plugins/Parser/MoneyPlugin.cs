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
    // Money plugin simply allows the $ to be prefixed to numbers.
    
    var salary = $225.50;
    if salary is more than $160 then
        print I worked overtime.
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling dates.
    /// </summary>
    public class MoneyPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public MoneyPlugin()
        {
            this.StartTokens = new string[] { "$" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "'$'<number>";
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
                    "$30",
                    "$50.23"
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
            // token has to be a literal token.
            var next = _tokenIt.Peek();
            if ( ! (next.Token.IsLiteralAny())) return false;
            if ( ! (next.Token.Type == TokenTypes.LiteralNumber)) return false;

            return true;
        }


        /// <summary>
        /// Parses the money expression in form $number
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // $<number>
            var startToken = _tokenIt.NextToken;
            _tokenIt.Advance();
            var val = _tokenIt.ExpectNumber();
            return Exprs.Const(new LNumber(val), startToken);
        }
    }
}
