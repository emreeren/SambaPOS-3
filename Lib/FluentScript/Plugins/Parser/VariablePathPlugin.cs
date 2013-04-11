using System.Collections.Generic;
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
    // Provides representation of file paths in more convenient ways
    
    // Set the home dir
    home    = c:\myapp\
    script  = "build"
    
    // Case 1: Typical approach
    file = home + "\\build\\build.xml"    
    
    // Case 2: VariablePath plugin simple case
    // The @ is optional on the first variable in the path.
    file = home\build\script.xml
    file = @home\build\script.xml
     
    // Case 3: Use variables in the path by prefixing @
    file = @home\build\@script.xml
    file = @home\build\@script-build.xml
    file = @home\build\@script.build.xml
     
    // Case 4: Use variables by prefixing with @ and surrounding by {}
    // This is only needed when separating a variable name from another variable name.
    file = @home\build\@{script}build.xml
    file = @home\build\@{script}-build.xml
    file = @home\build\@{script}.build.xml
     
     
    // NOTE:
    // This plugin is useful since the beginning drive letter ( "c:\" or "d:\" )
    // should NOT be hardcoded. This plugin removes the need to:
    // 1. use "+" for appending parts of the path
    // 2. use doublequotes for wrapping the path name e.g. "\build\appbuild.fs"
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin to allow using units such as length, weight, etc.
    /// </summary>
    public class VariablePathPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public VariablePathPlugin()
        {
            this.StartTokens = new string[] { "@", "$IdToken" };
            this.IsStatement = true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "@? '\\' ( ( <idtoken> | <numbertoken> | '-' | '.' )*  '\\'?)";
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
                    @"home\scripts\build.xml",
                    @"@home\scripts\@scriptname.xml",
                    @"@home\versions_@ver\scripts\build.xml"
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
            if (current != Tokens.At && !(current.Kind == TokenKind.Ident))
                return false;

            Token idToken = current, afterIdToken = null;
            if (current == Tokens.At)
            {
                idToken = _tokenIt.Peek().Token;
                afterIdToken = _tokenIt.Peek(2).Token;
            }
            else if (current.Kind == TokenKind.Ident)
            {
                afterIdToken = _tokenIt.Peek(1).Token;
            }
            return afterIdToken == Tokens.BackSlash;
        }


        /// <summary>
        /// Sorts expression
        /// </summary>
        /// <returns></returns>
        public override Expr  Parse()
        {
            var pathExps = new List<Expr>();
            var variableToken = _tokenIt.NextToken;
            
            // Move past the @ if it is first.
            if (variableToken.Token == Tokens.At) 
                variableToken = _tokenIt.Advance();

            // 1. Create a variable expression from the first idtoken
            AppendPathPart(pathExps, variableToken, null, false);                

            var path = string.Empty;
            var lastStringBasedPathToken = _tokenIt.Advance();
            var resetLastPathToken = false;

            while(!_tokenIt.IsEnded && !_tokenIt.IsEndOfStmtOrBlock())
            {
                var td = _tokenIt.NextToken;
                var token = td.Token;

                // Case 1: "\" - backslash is part of path
                if (token == Tokens.BackSlash)
                {
                    path += token.Text;
                }
                // Case 2: Id token \userdata\
                else if (token.Kind == TokenKind.Ident)
                {
                    path += token.Text;
                }
                // Case 3: "." or "-" e.g. \build.xml or \build-all.xml
                else if (token == Tokens.Dot || token == Tokens.Minus)
                {
                    path += token.Text;
                }
                // Case 4: 2.5 e.g. \version_2.5\config.xml
                else if ((token.Type == TokenTypes.LiteralNumber))
                {
                    path += token.Text;
                }
                // Case 3: @ token indicating that a variable name is next.
                else if (token == Tokens.At)
                {
                    CaptureVariableReference(pathExps, lastStringBasedPathToken, path);
                    resetLastPathToken = true;
                }                
                // Separator for function calls, arrays, dictionarys, endof statement
                else
                    break;

                _tokenIt.Advance();

                // This store the starting token of the last string based path
                if (resetLastPathToken)
                {
                    lastStringBasedPathToken = _tokenIt.NextToken;
                    path = string.Empty;
                    resetLastPathToken = false;
                }
            }
            if(!string.IsNullOrEmpty(path))
                AppendPathPart(pathExps, lastStringBasedPathToken, path, true);    

            var exp = ConstructBinaryExpr(pathExps);
            return exp;
        }


        private void AppendPathPart(List<Expr> pathExps, TokenData token, string text, bool isConstant)
        {
            if (string.IsNullOrEmpty(text))
                text = token.Token.Text;

            var start = isConstant ? Exprs.Const(new LString(text), _tokenIt.NextToken) as Expr
                                    : Exprs.Ident(text, _tokenIt.NextToken) as Expr;
            _parser.SetupContext(start, token);
            pathExps.Add(start);
        }


        private void CaptureVariableReference(List<Expr> pathExps, TokenData lastPathToken, string path)
        {
            // Add existing path as a constant expr.
            if (!string.IsNullOrEmpty(path))
                AppendPathPart(pathExps, lastPathToken, path, true);

            // Move past the "@"
            var n = _tokenIt.Advance();

            // CASE 1: "{" - Brace indicates an expression in between "{" and "}"
            // e.g. @{file.name}
            if (n.Token == Tokens.LeftBrace)
            {
                _tokenIt.Advance();
                var exp = _parser.ParseExpression(null, true, true, false, false);
                pathExps.Add(exp);
                if (_tokenIt.NextToken.Token != Tokens.RightBrace)
                    throw _tokenIt.BuildSyntaxExpectedTokenException(Tokens.RightBrace);

                return;
            }

            // CASE 2: Next token is id expression.
            AppendPathPart(pathExps, n, n.Token.Text, false);
        }


        private Expr ConstructBinaryExpr(List<Expr> pathExps)
        {
            // Case 1: Simple addition of @home and "\build\script.xml" as in "@home\build\script.xml"
            if (pathExps.Count == 2)
            {
                return Exprs.Binary(pathExps[0], Operator.Add, pathExps[1], pathExps[0].Token);
            }
            // Case 2: Add up all the expressions.
            // Start with the last 2 and keep adding backwards.
            // e.g. 0 1 2 3 
            // Exp1: Bin( 2, add, 3 )
            // Exp2: Bin( 1, Exp1 )
            // Exp3: Bin( 0, Exp2 )
            // e.g.  Bin( 0, add, Bin( 1, add, Bin( 2, add, 3 ) ) )
            var lastIndex = pathExps.Count - 1;
            var left =  pathExps[lastIndex - 1];
            var right = pathExps[lastIndex];
            var exp = Exprs.Binary(left, Operator.Add, right, left.Token);
            
            for (var ndx = lastIndex - 2; ndx >= 0; ndx--)
            {
                left = pathExps[ndx];
                exp = Exprs.Binary(left, Operator.Add, exp, left.Token);
            }
            return exp;
        }
    }
}