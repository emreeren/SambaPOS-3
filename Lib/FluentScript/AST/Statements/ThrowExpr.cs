
namespace ComLib.Lang.AST
{

    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class ThrowExpr : Expr
    {
        /// <summary>
        /// Create new instance
        /// </summary>
        public ThrowExpr()
        {
            this.Nodetype = NodeTypes.SysThrow;
        }


        /// <summary>
        /// Name for the error in the catch clause.
        /// </summary>
        public Expr Exp;


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitThrow(this);
        }
    }
}
