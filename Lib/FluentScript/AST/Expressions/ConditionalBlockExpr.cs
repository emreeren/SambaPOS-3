using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang
{
    /// <summary>
    /// Conditional based block statement used in ifs/elses/while
    /// </summary>
    public class ConditionalBlockExpr : BlockExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="statements"></param>
        public ConditionalBlockExpr(Expr condition, List<Expr> statements)
        {
            this.Condition = condition;
            this._statements = statements == null ? new List<Expr>() : statements;
        }


        /// <summary>
        /// The condition to check.
        /// </summary>
        public Expr Condition;
    }
}
