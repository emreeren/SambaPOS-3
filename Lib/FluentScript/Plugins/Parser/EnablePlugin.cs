using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Enables the units components that are built into the system. 
    // These include
    //   
    //   Abbr:   Name:
    //   in      inch
    //   in      inches
    //   ft      foot   
    //   ft      feet   
    //   yd      yard   
    //   yd      yards 
    //   mi      mile 
    //   mi      miles 
    //
    //   B       bytes
    //   kb      kilobyte
    //   kb      kilobytes
    //   mb      megabyte
    //   mb      megabytes
    //   gig     gigabyte
    //   gigs    gigabytes
    //
    //   oz      ounces    
    //   lb      pound     
    //   lbs     pounds    
    //   tn      ton       
    //   tn      tons      
    //   mg      milligram 
    //   mg      milligrams
    //   g       gram      
    //   g       grams     
    //   kg      kilogram  
    //   kg      kilograms 
    //   t       tonne     
    //   t       tonnes  
    
    enable units;
    var result = 3 inches + 5 feet + 2 yards + 1 mile;
     
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for enabling and disabling the 
    /// </summary>
    public class EnablePlugin : ExprPlugin
    {
        private static Dictionary<string, bool> _components;

        /// <summary>
        /// Initialize
        /// </summary>
        static EnablePlugin()
        {
            _components = new Dictionary<string, bool>();
            _components["units"] = true;
        }


        /// <summary>
        /// Intialize.
        /// </summary>
        public EnablePlugin()
        {
            this.IsStatement = true;
            this.IsSystemLevel = true;
            this.IsEndOfStatementRequired = true;
            this.StartTokens = new string[] { "enable", "disable" };
        }


        /// <summary>
        /// Whether or not this can handle the token supplied.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var ahead = _tokenIt.Peek(1, false);
            if (_components.ContainsKey(ahead.Token.Text))
                return true;
            return false;
        }


        /// <summary>
        /// enable units;
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            bool enable = false;
            if (_tokenIt.NextToken.Token.Text == "enable")
                enable = true;

            _tokenIt.Advance(1);
            string component = _tokenIt.NextToken.Token.Text;

            if (component == "units")
            {
                _parser.Context.Units.IsEnabled = enable;
                _parser.Context.Units.RegisterAll();
            }

            // Move past this plugin.
            _tokenIt.Advance();
            return new EmptyExpr();
        }     
    }
}
