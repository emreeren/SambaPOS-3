using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
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
                    "run function 'clean up'"
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

            // run function 'name';
            // run function touser();
            if (_tokenIt.NextToken.Token == Tokens.Function)
                _tokenIt.Advance();

            // 'username'
            if (!(_tokenIt.NextToken.Token.IsLiteralAny() || _tokenIt.NextToken.Token.Kind == TokenKind.Ident))
                _tokenIt.BuildSyntaxExpectedException("identifier or string");

            string name = _tokenIt.NextToken.Token.Text;

            // Case 1: run 'step1';
            // Case 2: run step1;
            var next = _tokenIt.Peek();
            if (next.Token != Tokens.LeftParenthesis && next.Token != Tokens.Dot)
            {
                var funcExp = new FunctionCallExpr();
                funcExp.NameExp = new VariableExpr(name);
                _parser.State.FunctionCall++;
                
                //Ctx.Limits.CheckParserFuncCallNested(_tokenIt.NextToken, _parser.State.FunctionCall);
                _parser.State.FunctionCall--;

                // Move past this plugin
                _tokenIt.Advance();
                return funcExp;
            }

            // Case 3: run step1();
            Expr exp = _parser.ParseIdExpression(name);
            return exp;
        }
    }
}
