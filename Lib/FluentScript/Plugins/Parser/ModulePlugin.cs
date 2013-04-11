using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Module plugin allows you to create modules ( namespaces ) with the word "mod"
    
    mod math
    {
        function min( a, b )
        {
            if( a <= b ) return a
            return b
        }
    }
    
    
    var min = math.min( 2, 3 )
    
    </doc:example>
    ***************************************************************************/
    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class ModulePlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public ModulePlugin()
        {
            this.IsAutoMatched = true;
            this.StartTokens = new string[] { "mod" };
            this.IsStatement = true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "mod '{' <statement_list> '}'"; }
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
                    "mod math { function inc( a ) { return a + 1; } }"
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // Move past "mod"
            _tokenIt.Advance();

            // Get the name of the module "e.g." "math"
            var name = _tokenIt.ExpectId();

            // 1. Create the symbol to represent module
            var symbol = new SymbolModule();
            symbol.Name = name;
            symbol.Category = SymbolCategory.Module;
            symbol.DataType = new LModuleType();
            symbol.DataType.Name = name;
            symbol.DataType.FullName = name;
            symbol.Scope = new SymbolsNested(name);
            symbol.ParentScope = this.Ctx.Symbols.Current;

            // 2. Add the module symbol to the current scope
            this.Ctx.Symbols.Define(symbol);

            // 3. Now push the scope on top of the current scope. ( since modules can be nested )
            this.Ctx.Symbols.Push(symbol.Scope, true);

            var block = new BlockExpr();
            _parser.ParseBlock(block);
            this.Ctx.Symbols.Pop();
            return block;
        }
    }
}