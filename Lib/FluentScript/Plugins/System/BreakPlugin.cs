using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public class BreakPlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public BreakPlugin()
        {
            this.ConfigureAsSystemStatement(false, true, "break");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "break <statementterminator>"; }
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
                    "break;",
                    "break\r\n"
                };
            }
        }


        /// <summary>
        /// break;
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var expr = new BreakExpr();
            _tokenIt.Expect(Tokens.Break);
            return expr;
        }
    }
}
