using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
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
    public class TryCatchPlugin : ExprBlockPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public TryCatchPlugin()
        {
            this.ConfigureAsSystemStatement(true, false, "try");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "try <statementblock> catch '(' <id> ')' <statementblock>"; }
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
                    "try { add(1, 2); } catch( err ) { print( err.name ); }"
                };
            }
        }


        /// <summary>
        /// try/catch.
        /// </summary>
        /// <returns></returns>
        public override Expr  Parse()
        {
            var stmt = new TryCatchExpr();
            var statements = new List<Expr>();

            _tokenIt.Expect(Tokens.Try);
            ParseBlock(stmt);
            _tokenIt.AdvancePastNewLines();
            _tokenIt.ExpectMany(Tokens.Catch, Tokens.LeftParenthesis);
            stmt.ErrorName = _tokenIt.ExpectId();
            _tokenIt.Expect(Tokens.RightParenthesis);
            ParseBlock(stmt.Catch);
            stmt.Ctx = Ctx;
            stmt.Catch.Ctx = Ctx;
            return stmt;
        }
    }
}
