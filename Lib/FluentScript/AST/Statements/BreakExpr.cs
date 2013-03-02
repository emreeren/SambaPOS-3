

namespace ComLib.Lang.AST
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class BreakExpr : Expr
    {
        public BreakExpr()
        {
            this.Nodetype = NodeTypes.SysBreak;
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitBreak(this);
        }
    }
}
