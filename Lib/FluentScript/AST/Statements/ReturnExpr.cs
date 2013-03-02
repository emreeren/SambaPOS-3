
namespace ComLib.Lang.AST
{

    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class ReturnExpr : Expr
    {
        public ReturnExpr()
        {
            this.Nodetype = NodeTypes.SysReturn;
        }


        /// <summary>
        /// Return value.
        /// </summary>
        public Expr Exp;


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitReturn(this);
        }
    }
}
