using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Parsing;
// </lang:using>

namespace ComLib.Lang.Plugins
{

    /* *************************************************************************
    <doc:example>	
    // AndOr plugin allows the words "and" "or" to be used in place of && ||
    
    if ( i > 30 and j < 20 ) then print works
    if ( i < 30 or  j > 20 ) then print works        
        
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class AndOrPlugin : AliasTokenPlugin
    {        
        /// <summary>
        /// Initialize
        /// </summary>
        public AndOrPlugin() : base("and", ComLib.Lang.Core.Tokens.LogicalAnd )
        {
            Register("or", ComLib.Lang.Core.Tokens.LogicalOr);
            _tokens = new string[] { "and", "or" };
        }
    }
}
