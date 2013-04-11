using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Fluent Func plugins allows calling functions with spaces.
     
    
    // @summary: finds doctors based on the zip code provided and 
    // the specicality and accepted insurance
    // @arg: name: zip,       type: number, desc: The zipcode, examples: 11201
    // @arg: name: specialty, type: text,   desc: Specialization, examples: 'Familye'
    // @arg: name: insurance, type: text,   desc: Name of insurance, examples: 'empire'
    function find_doctors_by_zipcode ( zip, specialty, insurance )
    {
	    // ... some code here.
    }
    
    
    // @arg: name: product, desc: The product id,  type: text, examples: 'AS-1232'
    // @arg: name: amount,  desc: Number of items, type: text, examples: 23
    function refill_inventory( product, amount )
    {
        // ... some code here.
    }
    
    
    // Call functions replacing "_" with space - parameters are optional if function call
    // is on a single line.
    
    // Example 1:
    find doctors by zipcode 11200, 'family practice', 'empire insurance'
     
    // Example 2:
    refill inventory 'KL-131', 200
    
    </doc:example>
    ***************************************************************************/


    /// <summary>
    /// Combinator for handles method/function calls in a more fluent way.
    /// </summary>
    public class FluentFuncPlugin : ExprPlugin
    {
        private FunctionLookupResult _result;


        /// <summary>
        /// Initialize.
        /// </summary>
        public FluentFuncPlugin()
        {
            this.Precedence = 100;
            this.IsStatement = true;
            this.StartTokens = new string[] { "$IdToken" };
        }


        /// <summary>
        /// This can not handle all idtoken based expressions.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            if (!(current.Kind == TokenKind.Ident)) return false;

            var next = _tokenIt.Peek(1, false);
            if (!(next.Token.Kind == TokenKind.Ident)) return false;

            // Check if multi-word function name.
            var ids = _tokenIt.PeekConsequetiveIdsAppendedWithTokenCounts(true, _tokenIt.LLK);
            _result = FluentHelper.MatchFunctionName(_parser.Context, ids);
            return _result.Exists;
        }


        /// <summary>
        /// Parses the fluent function call.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // 1. Is it a function call?
            var nameExp = Exprs.Ident(_result.Name, null);
            
            _tokenIt.Advance(_result.TokenCount);
            var exp = _parser.ParseFuncExpression(nameExp, null);
            return exp;
        }
    }
}