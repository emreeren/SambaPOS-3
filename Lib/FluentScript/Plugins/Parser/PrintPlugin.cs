using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
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
            if (nextToken.Token == ComLib.Lang.Tokens.LeftParenthesis || nextToken.Token.Text == "\"")
                return false;
            return true;
        }


        /// <summary>
        /// Parse the expression.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            var includeNewLine = false;
            if (_lexer.LastTokenData.Token.Text == "println")
                includeNewLine = true;
            return base.ParseLine(includeNewLine);
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
                lineExp = new ConstantExpr(lineToken.Value);

            var exp = new FunctionCallExpr();
            exp.NameExp = new VariableExpr("print");   
            exp.ParamListExpressions.Add(lineExp);
            _parser.SetScriptPosition(exp, printToken);

            // Move past this plugin.
            _tokenIt.Advance();
            return exp;
        }
    }
}
