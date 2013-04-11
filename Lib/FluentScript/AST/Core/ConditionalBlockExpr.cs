
using System.Collections.Generic;

namespace Fluentscript.Lib.AST.Core
{
    /// <summary>
    /// Conditional based block statement used in ifs/elses/while
    /// </summary>
    public class ConditionalBlockExpr : BlockExpr
    {
        public ConditionalBlockExpr()
        {

        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="statements"></param>
        public ConditionalBlockExpr(Expr condition, List<Expr> statements)
        {
            this.Condition = condition;
            this._statements = statements ?? new List<Expr>();
        }


        /// <summary>
        /// The condition to check.
        /// </summary>
        public Expr Condition;
    }
}
