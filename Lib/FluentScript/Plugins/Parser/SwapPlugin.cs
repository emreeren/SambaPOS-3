using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Parser.PluginSupport;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Swap plugin provides 1 line statement to swap variables.
    
    var a = 1, b = 2;
    
    // Swap values in 1 statement.
    // Instead of needing a third variable.
    swap a with b;
    
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling swapping of variable values. swap a and b.
    /// </summary>
    public class SwapPlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public SwapPlugin()
        {
            this.IsStatement = true;
            this.IsEndOfStatementRequired = true;
            this.IsAutoMatched = true;
            this.StartTokens = new string[] { "swap", "Swap" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "swap <id> with <id> <statementtermninator>";
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
                    "swap a with b"
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            _tokenIt.Advance();
            var name1 = _tokenIt.ExpectId(true);
            _tokenIt.ExpectIdText("with");
            var name2 = _tokenIt.ExpectId();
            return new SwapExpr(name1, name2);
        }
    }


    /// <summary>
    /// Variable expression data
    /// </summary>
    public class SwapExpr : Expr
    {
        private string _name1;
        private string _name2;

        /// <summary>
        /// Initialize with names of variables to swap.
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        public SwapExpr(string name1, string name2)
        {
            _name1 = name1;
            _name2 = name2;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            // var a = 1;
            // var b = 2;
            // swap a with b;
            // var temp = a;
            // a = b;
            // b = temp;
            var val1 = this.Ctx.Memory.Get<object>(_name1);
            var val2 = this.Ctx.Memory.Get<object>(_name2);
            this.Ctx.Memory.SetValue(_name1, val2);
            this.Ctx.Memory.SetValue(_name2, val1);
            return val2;
        }
    }
}
