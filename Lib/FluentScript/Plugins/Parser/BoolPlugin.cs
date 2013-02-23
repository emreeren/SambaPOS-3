

// <lang:using>
using ComLib.Lang.Core;
// </lang:using>

namespace ComLib.Lang.Plugins
{

    /* *************************************************************************
    <doc:example>	
    // Bool plugin allows aliases for true/false
    
    var result = on;
    var result = off;
    var result = yes;
    var result = no;
    </doc:example>
    ***************************************************************************/
    
    
    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class BoolPlugin : AliasTokenPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public BoolPlugin()
            : base("yes", Tokens.True)
        {
            Register("Yes", Tokens.True);
            Register("on",  Tokens.True);
            Register("On",  Tokens.True);
            Register("no",  Tokens.False);
            Register("No",  Tokens.False); 
            Register("off", Tokens.False);
            Register("Off", Tokens.False);            
            _tokens = new string[] { "yes", "Yes", "no", "No", "on", "On", "off", "Off" };
        }
    }
}
