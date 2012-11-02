using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
{

    /* *************************************************************************
    <doc:example>	
    // Step plugin allows the word "step" to be used instead of "function" when declaring functions.
        
    step add( a, b ) 
    { 
        return a + b
    }
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class StepPlugin : AliasTokenPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public StepPlugin() : base("step", ComLib.Lang.Tokens.Function )
        {
        }
    }
}
