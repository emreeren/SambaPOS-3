using System;
using Fluentscript.Lib.AST.Core;
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
    // Named index plugin allows numeric access to array items using non-0 based index 
    // and in a fluent approach. 
    
    var items = [1, 2, 3, 4, 5]
    var result = 2nd item
    
    // Note in the example above 3 things:
    // 1. "2nd" represents the index to access. this is equivalent to items[1]
    // 2. if variable is "items"( plural ), you can type "item"( singular )
    // 3. This is always 1 based. e.g. 3rd item == items[2]
    
    </doc:example>
    ***************************************************************************/
    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class NamedIndexPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public NamedIndexPlugin()
        {
            this.IsAutoMatched = true;
            this.StartTokens = new string[] { "$NumberToken" };
        }


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var n = _tokenIt.Peek().Token;
            var nextText = n.Text;
            if (nextText != "st" && nextText != "nd" && nextText != "rd" && nextText != "th")
                return false;

            n = _tokenIt.Peek(2).Token;
            if (n.Kind != TokenKind.Ident)
                return false;

            // Finally check if there is a plural symbol that exists.
            var id = n.Text;
            if (this.Ctx.Symbols.Contains(id + "s"))
                return true;
            return false;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "<number> ( 'st' | 'nd' | 'rd' | 'th' ) <ident>"; }
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
                    "1st item",
                    "2nd item",
                    "3rd item",
                    "4th item",
                    "23rd item"
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var startToken = _tokenIt.NextToken;
            // 1. index number: it's 1 based so substract 1.
            var index = Convert.ToDouble(_tokenIt.NextToken.Token.Text) - 1;
            var indexExpr = Exprs.Const(new LNumber(index), _tokenIt.NextToken);
            _tokenIt.Advance();

            // 2. "st" or "nd" or "rd" or "th"
            _tokenIt.Advance();

            // 3. identifier
            var ident = _tokenIt.NextToken.Token.Text + "s";
            var identExpr = Exprs.Ident(ident, _tokenIt.NextToken);
            _tokenIt.Advance();

            return Exprs.Index(identExpr, indexExpr, false, startToken);

        }
    }
}