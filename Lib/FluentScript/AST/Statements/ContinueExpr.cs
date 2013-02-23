
namespace ComLib.Lang.AST
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class ContinueExpr : Expr
    {
        public ContinueExpr()
        {
            this.Nodetype = NodeTypes.SysContinue;
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitContinue(this);
        }
    }
}
