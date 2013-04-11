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
    // Run plugin provides alternate way to call a function for fluid syntax.
    // Notes: 
    // 1. The keyword "function" can be aliased with the word "step"
    // 2. The name of a function can be in quotes with spaces.
    
    // This is a function with 0 parameters so parentheses are not required
    step Cleanup
    {
        // do something here.
    }
     
    
    // This is a function with string for name and 0 parameters so parentheses are not required
    step 'Clean up'
    {
        // do something here.
    }
    
    // Example 1: Call function normally
    Cleanup();
    
    // Example 2: Call function using Run keyword
    run Cleanup();
    
    // Example 3: Call function using run without parenthesis for function name.
    run Cleanup;
    
    // Example 4: Call function with spaces in name using run with quotes for function name.    
    run 'Clean up';
    
    // Example 5: Call function with spaces using run and keyword.
    run step 'Clean up';
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for function calls using "run" keyword first.
    /// </summary>
    public class RunPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public RunPlugin()
        {
            this.IsStatement = true;
            this.IsAutoMatched = true;
            this.StartTokens = new string[] { "run", "Run" };
            this.IsEndOfStatementRequired = true;

        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "run function? ( <id> | <stringliteral> ) <paramlist>";
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
                    "run cleanup",
                    "run function cleanup",
                    "run cleanup()",
                    "run 'clean up'",
                    "run 'clean up'()",
                    "run cleanup('tempdir') on initialize()"
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            _tokenIt.Expect(Tokens.Run);

            // 1. Expect run ( function | step  )
            if (_tokenIt.NextToken.Token == Tokens.Function || _tokenIt.NextToken.Token.Text == "step")
                _tokenIt.Advance();

            // 2. Expect identifier for function name
            if (!(_tokenIt.NextToken.Token.IsLiteralAny() || _tokenIt.NextToken.Token.Kind == TokenKind.Ident))
                _tokenIt.BuildSyntaxExpectedException("identifier or string");

            // 3. get the name of the function to run.
            var name = _tokenIt.NextToken.Token.Text;
            var nameToken = _tokenIt.NextToken;
            var next = _tokenIt.Peek(1, false);

            // Case 1: run step cleanup ; | newline | eof
            if (Terminators.ExpFlexibleEnd.ContainsKey(next.Token) || next.Token == Tokens.EndToken)
            {
                var nameExp = Exprs.Ident(name, nameToken);
                var funcExp = Exprs.FunctionCall(nameExp, null, nameToken);

                // Move past token
                _tokenIt.Advance();
                return funcExp;
            }

            // Case 2: run step cleanup on <functioncall>
            if (next.Token.Text == "on" || next.Token.Text == "after")
            {
                _tokenIt.Advance(1);
                var runExp = ParseRunExpr(nameToken);
                var nameExp = Exprs.Ident(name, nameToken);
                var funcExp = Exprs.FunctionCall(nameExp, null, nameToken);
                runExp.FuncCallOnAfterExpr = funcExp;
                runExp.Mode = next.Token.Text;
                return runExp;
            }

            // Case 3: run step cleanup('c:\tempdir') on <functioncall>
            if(next.Token == Tokens.LeftParenthesis || next.Token == Tokens.Dot )
            {
                var funcExp = _parser.ParseIdExpression(name, null, false);

                if (_tokenIt.NextToken.Token.Text == "on" || _tokenIt.NextToken.Token.Text == "after")
                {
                    var t = _tokenIt.NextToken;
                    var runExp = ParseRunExpr(nameToken);
                    runExp.FuncCallOnAfterExpr = funcExp;
                    runExp.Mode = t.Token.Text;
                    return runExp;
                }
                return funcExp;
            }
            // Some other token e.g. + - < etc.
            var funcname = Exprs.Ident(name, nameToken);
            var funcExp2 = Exprs.FunctionCall(funcname, null, nameToken);

            // Move past token
            _tokenIt.Advance();
            return funcExp2;

            //throw this.TokenIt.BuildSyntaxUnexpectedTokenException();
        }


        private RunExpr ParseRunExpr(TokenData startToken)
        {
            // Move past "on"
            _tokenIt.Advance(1);            
            var exp = _parser.ParseIdExpression(string.Empty, null, false);
            var runexp = Exprs.Run(string.Empty, exp, startToken) as RunExpr;
            return runexp;
        }
    }
}
