using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
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
            : base("yes", ComLib.Lang.Tokens.True)
        {
            Register("Yes", ComLib.Lang.Tokens.True);
            Register("on",  ComLib.Lang.Tokens.True);
            Register("On",  ComLib.Lang.Tokens.True);
            Register("no",  ComLib.Lang.Tokens.False);
            Register("No",  ComLib.Lang.Tokens.False); 
            Register("off", ComLib.Lang.Tokens.False);
            Register("Off", ComLib.Lang.Tokens.False);            
            _tokens = new string[] { "yes", "Yes", "no", "No", "on", "On", "off", "Off" };
        }
    }
}
