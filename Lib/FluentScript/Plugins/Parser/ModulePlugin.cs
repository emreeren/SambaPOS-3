using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Helpers;
using ComLib.Lang.Types;
using ComLib.Lang.Parsing;
// </lang:using>

namespace ComLib.Lang.Plugins
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
            symbol.Category = SymbolCategory.CustomScope;
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