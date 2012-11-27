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
            : base("yes", ComLib.Lang.Core.Tokens.True)
        {
            Register("Yes", ComLib.Lang.Core.Tokens.True);
            Register("on",  ComLib.Lang.Core.Tokens.True);
            Register("On",  ComLib.Lang.Core.Tokens.True);
            Register("no",  ComLib.Lang.Core.Tokens.False);
            Register("No",  ComLib.Lang.Core.Tokens.False); 
            Register("off", ComLib.Lang.Core.Tokens.False);
            Register("Off", ComLib.Lang.Core.Tokens.False);            
            _tokens = new string[] { "yes", "Yes", "no", "No", "on", "On", "off", "Off" };
        }
    }
}
