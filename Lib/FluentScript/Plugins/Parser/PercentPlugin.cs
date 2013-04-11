using Fluentscript.Lib.AST;
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
    // Percent plugin allow using a percent sign % after a number to indicate percentage
     
    a = 25%
    b = 75 %
     
    // NOTES
    // 1. You can not use this when an identifier comes after the "%"
    // 2. You can not use this when another number comes after the "%"
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin to allow using units such as length, weight, etc.
    /// </summary>
    public class PercentPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public PercentPlugin()
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
                return "<numbertoken> %";
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
                    "25%",
                    "25 %"
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
            if (t.Token != Tokens.Percent) return false;

            // Now check that the token after % is not a number or a ident.
            // e.g. The following would indicate doing a modulo operation
            // 1. 25 % 4
            // 2. 25 % result            
            t = _tokenIt.Peek(2, false);
            if (t.Token.IsLiteralAny()) return false;
            if (t.Token.Kind == TokenKind.Ident) return false;
            //if (t.Token is LiteralToken)
            //{                
            //    if (((LiteralToken)t.Token).IsNumeric())
            //        return false;
            //}
            return true;
        }


        /// <summary>
        /// Sorts expression
        /// </summary>
        /// <returns></returns>
        public override Expr Parse(object context)
        {
            var startToken = _tokenIt.NextToken;
            var constExp = context as ConstantExpr;
            var ctx = _parser.Context;

            var percentToken = _tokenIt.Peek(1);
            var numberVal = (LObject)constExp.Value;
            // Validate 
            if (numberVal.Type != LTypes.Number)
                throw _tokenIt.BuildSyntaxException("number required when percentage( % ) : " + percentToken.Token.Text, percentToken);

            // Move past the current "%" token.
            var t = _tokenIt.Advance(2);

            var val = ((LNumber)numberVal).Value;
            val = val / 100;
            var finalExp = Exprs.Const(new LNumber(val), startToken);
            return finalExp;
        }
    }
}