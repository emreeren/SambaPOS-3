

// <lang:using>
using ComLib.Lang.Core;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class UnaryExpr : VariableExpr
    {
        /// <summary>
        /// The increment value.
        /// </summary>
        public double Increment;


        /// <summary>
        /// The operator.
        /// </summary>
        public Operator Op;


        /// <summary>
        /// The expression to apply a unary symbol on. e.g. !
        /// </summary>
        public Expr Expression;


        /// <summary>
        /// Initialize
        /// </summary>
        public UnaryExpr()
        {
            this.Nodetype = NodeTypes.SysUnary;
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitUnary(this);
        }
    }
}
