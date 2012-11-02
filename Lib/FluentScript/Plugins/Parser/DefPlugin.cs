using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
{

    /* *************************************************************************
    <doc:example>	
    // Def plugin allows the word "def" to be used instead of "function" when declaring functions.
        
    def add( a, b ) 
    { 
        return a + b
    }
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class DefPlugin : AliasTokenPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public DefPlugin() : base("def", ComLib.Lang.Tokens.Function )
        {
        }
    }
}
