using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Parsing;
using ComLib.Lang.Types;

// </lang:using>

namespace ComLib.Lang.Plugins
{

    /* *************************************************************************
    <doc:example>	
    // Throw plugin provides throwing of errors from the script.
    
    throw 'user name is required';
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin for throwing errors from the script.
    /// </summary>
    public class ThrowPlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public ThrowPlugin()
        {
            this.ConfigureAsSystemStatement(false, true, "throw");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "throw <expression> <statementterminator>"; }
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
                    "throw 'invalid amount';",
                    "throw 300\r\n"
                };
            }
        }


        /// <summary>
        /// throw error;
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
             _tokenIt.Expect(Tokens.Throw);
            var exp = _parser.ParseExpression(Terminators.ExpStatementEnd, passNewLine: true);
            return new ThrowExpr() { Exp = exp };
        }
    }
}
