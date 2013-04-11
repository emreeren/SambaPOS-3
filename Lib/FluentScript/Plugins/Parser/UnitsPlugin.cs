using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Units plugin allows the usage of units of measure such as length, weight
    
    enable units;

    // inches, bytes are base types. everything is relative to the base types
    // In examples below, the units will get converted to the measurement on the
    // left hand side. e.g. 3 feet + 5 inches will get converted to datatype
    // LUnit with value in feet and basevalue in inches.    
    var result1 = 3 feet + 5 inches + 10 yards + 2 miles
    var result2 = 1 meg + 30 kb + 50 B + 2 gigs

    // Each unit of measure is based on a basevalue with a relative value.
    // e.g. 
    // type             basevalue   relative values
    // length           inches      feet, yard, mile
    // weight           ounces      milligrams, grams, kilograms
    // computerspace    bytes       kilobytes, megabytes, gigabytes
   
    print feet: #{result1.Value} , inches: #{result1.BaseValue}
    print megs: #{result2.Value} , bytes:  #{result2.BaseValue}
     
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin to allow using units such as length, weight, etc.
    /// </summary>
    public class UnitsPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public UnitsPlugin()
        {
            this.StartTokens = new string[] { "$Suffix" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "<literal> <identifier>";
            }
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
                    "3 inches",
                    "20 feet"
                };
            }
        }


        /// <summary>
        /// Whether or not this plugin can handle current token(s).
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var t = _tokenIt.Peek(1, false);
            if (_parser.Context.Units.Contains(t.Token.Text))
                return true;
            return false;
        }


        /// <summary>
        /// Sorts expression
        /// </summary>
        /// <returns></returns>
        public override Expr Parse(object context)
        {
            var constExp = context as ConstantExpr;
            var ctx = _parser.Context;
            var t = _tokenIt.Advance();
            var lobj = (LObject)constExp.Value;
            // Validate.
            if (lobj.Type != LTypes.Number)
                throw _tokenIt.BuildSyntaxException("number required when using units : " + t.Token.Text, t);

            var lval = ((LNumber)lobj).Value;
            var result = ctx.Units.ConvertToUnits(lval, t.Token.Text);
            var lclass = LangTypeHelper.ConvertToLangUnit(result);
            var finalExp = Exprs.Const(lclass, t);
            
            // Move past the plugin.
            _tokenIt.Advance();
            return finalExp;
        }
    }
}