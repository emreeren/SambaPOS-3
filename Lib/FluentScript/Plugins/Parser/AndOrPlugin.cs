using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
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
        public AndOrPlugin() : base("and", ComLib.Lang.Tokens.LogicalAnd )
        {
            Register("or", ComLib.Lang.Tokens.LogicalOr);
            _tokens = new string[] { "and", "or" };
        }
    }
}
