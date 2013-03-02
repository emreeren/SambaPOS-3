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
    public class WhilePlugin : ExprBlockPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public WhilePlugin()
        {
            this.ConfigureAsSystemStatement(true, false, "while");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "while ( ( <expression> then <statementblock> ) | ( '(' <expression> ')' <statementblock> ) )"; }
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
                    "while count < 20 then print( 'hi' );",
                    "while count < 20 then { print( 'hi' ); }",
                    "while ( count < 20 )   print( 'hi' );",
                    "while ( count < 20 ) { print( 'hi' ); }",
                };
            }
        }


        /// <summary>
        /// Parses either the for or for x in statements.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var stmt = new WhileExpr();
            // While ( condition expression )
            _tokenIt.Expect(Tokens.While);
            ParseConditionalBlock(stmt);
            return stmt;
        }
    }
}
