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
using ComLib.Lang.Parsing;
// </lang:using>

namespace ComLib.Lang.Plugins
{
    /* *************************************************************************
    <doc:example>	
    // Return plugin provides return values
    
    return false;
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin for throwing errors from the script.
    /// </summary>
    public class ForLoopPlugin : ExprBlockPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public ForLoopPlugin()
        {
            this.ConfigureAsSystemStatement(true, false, "for");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "for '(' ( ( <id> in <expression> ) | ( <id> '=' <expression> ';' <id> <op> <expression> ';' <id> <op> <expression>? ) ) ')' <statementblock>"; }
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
                    "for ( num in numbers ) { ... }",
                    "for ( num = 1; num < 10; num++ ) { ... }",
                    "for ( num = 1; num < 10; num += 2 ) { ... }"
                };
            }
        }


        /// <summary>
        /// Parses either the for or for x in statements.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            _tokenIt.ExpectMany(Tokens.For, Tokens.LeftParenthesis);
            var ahead = _tokenIt.Peek(1);
            if (ahead.Token == Tokens.In) return ParseForIn();

            return ParseForLoop();
        }


        private Expr ParseForLoop()
        {
            var startToken = _tokenIt.NextToken;
            var start = _parser.ParseStatement();            
            var condition = _parser.ParseExpression(Terminators.ExpSemicolonEnd);
            _tokenIt.Advance();
            var name = _tokenIt.ExpectId();
            var increment = _parser.ParseUnary(name, false);
            _tokenIt.Expect(Tokens.RightParenthesis);
            var stmt = Exprs.For(start, condition, increment, startToken) as BlockExpr;
            ParseBlock(stmt);
            return stmt;
        }


        /// <summary>
        /// return value;
        /// </summary>
        /// <returns></returns>
        private Expr ParseForIn()
        {
            var startToken = _tokenIt.NextToken;
            var varname = _tokenIt.ExpectId();
            _tokenIt.Expect(Tokens.In);
            var sourcename = _tokenIt.ExpectId();
            _tokenIt.Expect(Tokens.RightParenthesis);
            var stmt = Exprs.ForEach(varname, sourcename, startToken) as BlockExpr;
            ParseBlock(stmt);
            return stmt;
        }
    }
}
