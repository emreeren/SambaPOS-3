using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
using ComLib.Lang.Parsing;
// </lang:using>

namespace ComLib.Lang.Plugins
{
    /// <summary>
    /// Plugin for throwing errors from the script.
    /// </summary>
    public class IfPlugin : ExprBlockPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public IfPlugin()
        {
            this.ConfigureAsSystemStatement(true, false, "if");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "if ( ( <expression> then <statementblock> ) | ( '(' <expression> ')' <statementblock> ) )"; }
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
                    "if true then print hi",
                    "if ( total > 10 ) print hi",
                    "if ( isActive && age >= 21  ) { print('fluent'); print('script'); }"
                };
            }
        }


        /// <summary>
        /// return value;
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {            
            var stmt = new IfExpr();
            var statements = new List<Expr>();

            // While ( condition expression )
            _tokenIt.Expect(Tokens.If);

            // Parse the if
            ParseConditionalBlock(stmt);
            _tokenIt.AdvancePastNewLines();

            // Handle "else if" and/or else
            if (_tokenIt.NextToken.Token == Tokens.Else)
            {
                // _tokenIt.NextToken = "else"
                _tokenIt.Advance();
                _tokenIt.AdvancePastNewLines();

                // What's after else? 
                // 1. "if"      = else if statement
                // 2. "{"       = multi  line else
                // 3. "nothing" = single line else
                // Peek 2nd token for else if.
                var token = _tokenIt.NextToken;
                if (_tokenIt.NextToken.Token == Tokens.If)
                {
                    stmt.Else = Parse() as BlockExpr;
                }
                else // Multi-line or single line else
                {
                    var elseStmt = new BlockExpr();
                    ParseBlock(elseStmt);
                    _parser.SetupContext(elseStmt, token);
                    stmt.Else = elseStmt;                    
                }
            }
            return stmt;
        }
    }
}
