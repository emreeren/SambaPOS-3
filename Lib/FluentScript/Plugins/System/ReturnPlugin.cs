using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Helpers;
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
    public class ReturnPlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public ReturnPlugin()
        {
            this.ConfigureAsSystemExpression(false, true, "return");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "return <expression> <statementterminator>"; }
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
                    "return 3;",
                    "return 3\r\n",
                    "return result",
                    "return add(1,2)"
                };
            }
        }


        /// <summary>
        /// return value;
        /// </summary>
        /// <returns></returns>
        public override Expr  Parse()
        {
            var stmt = new ReturnExpr();
            _tokenIt.Expect(Tokens.Return);
            if (_tokenIt.IsEndOfStmtOrBlock())
                return stmt;

            var exp = _parser.ParseExpression(Terminators.ExpStatementEnd, passNewLine: false);
            stmt.Exp = exp;
            return stmt;
        }
    }
}
