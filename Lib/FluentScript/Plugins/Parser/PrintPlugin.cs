using System;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Plugins.Core;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Print plugin derives from the "TakeoverPlugin"
    // Takeovers are keywords that consume the entire line of text in the script
    // after the keyword. 
    // In this case of the Print plugin, it consume the rest of the line and you
    // don't need to wrap text around quotes.
    
    var language = 'fluentscript';
    print No need for quotes in #{language} if text to print is on one line    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling boolean values in differnt formats (yes, Yes, no, No, off Off, on On).
    /// </summary>
    public class PrintPlugin : LineReaderPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public PrintPlugin()
        {
            _tokens = new string[] { "print", "println" };
        }


        /// <summary>
        /// Can only handle print if no ( and " supplied.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var nextToken = _lexer.PeekToken();
            if (nextToken.Token == Tokens.LeftParenthesis || nextToken.Token.Text == "\"")
                return false;
            return true;
        }


        /// <summary>
        /// Parse the expression.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            bool includeNewLine = _lexer.LastTokenData.Token.Text == "println";
            var resultTokens = base.ParseLine(includeNewLine);

            // Add new line to end if using "println"
            if(resultTokens.Length == 2 && includeNewLine)
            {
                var first = resultTokens[1];
                if(first.Kind != TokenKind.Multi)
                    first.SetTextAndValue(first.Text, first.Text + Environment.NewLine);
                
            }
            return resultTokens;
        }
    }



    /// <summary>
    /// Prints the next token.
    /// </summary>
    public class PrintExpressionPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        public PrintExpressionPlugin()
        {
            IsAutoMatched = true;
            IsStatement = true;
            _handleNewLineAsEndOfExpression = true;
            _startTokens = new string[] { "print", "println" };
        }


        /// <summary>
        /// Whether or not this plugin can handle the current token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var next = _tokenIt.Peek().Token;
            if (next == Tokens.LeftParenthesis)
                return false;
            return true;    
        }


        /// <summary>
        /// Parse the expression.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var printToken = _tokenIt.NextToken;
            var lineToken = _tokenIt.AdvanceAndGet<Token>();
            Expr lineExp = null;
            if (lineToken.Kind == TokenKind.Multi)
                lineExp = _parser.ParseInterpolatedExpression(lineToken);
            else
                lineExp = Exprs.Const(new LString((string)lineToken.Value), printToken);

            var nameExp = Exprs.Ident(printToken.Token.Text, printToken);
            var exp = (FunctionCallExpr)Exprs.FunctionCall(nameExp, null, printToken);
            exp.ParamListExpressions.Add(lineExp);
            
            // Move past this plugin.
            _tokenIt.Advance();
            return exp;
        }
    }
}
