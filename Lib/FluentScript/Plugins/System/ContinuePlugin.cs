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
    public class ContinuePlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public ContinuePlugin()
        {
            this.ConfigureAsSystemStatement(false, true, "continue");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "continue <statementterminator>"; }
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
                    "continue;",
                    "continue\r\n"
                };
            }
        }


        /// <summary>
        /// continue;
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var expr = new ContinueExpr();
            _tokenIt.Expect(Tokens.Continue);
            return expr;
        }
    }
}
