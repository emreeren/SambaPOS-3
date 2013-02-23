
// <lang:using>
using ComLib.Lang.Core;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Condition expression less, less than equal, more, more than equal etc.
    /// </summary>
    public class CompareExpr : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="left">Left hand expression</param>
        /// <param name="op">Operator</param>
        /// <param name="right">Right expression</param>
        public CompareExpr(Expr left, Operator op, Expr right)
        {
            this.Nodetype = NodeTypes.SysCompare;
            this.Left = left;
            this.Right = right;
            this.AddChild(left);
            this.AddChild(right);
            this.Op = op;
        }


        /// <summary>
        /// Left hand expression
        /// </summary>
        public Expr Left;


        /// <summary>
        /// Operator > >= == != less less than
        /// </summary>
        public Operator Op;


        /// <summary>
        /// Right hand expression
        /// </summary>
        public Expr Right;


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitCompare(this);
        }
    }    
}
