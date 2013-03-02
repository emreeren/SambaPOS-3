
// <lang:using>
// </lang:using>

namespace ComLib.Lang.Plugins
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
        public StepPlugin() : base("step", ComLib.Lang.Core.Tokens.Function )
        {
        }
    }
}
