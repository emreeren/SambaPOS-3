

namespace ComLib.Lang.AST
{
    public class LambdaExpr : Expr
    {
        public LambdaExpr()
        {
            this.Nodetype = NodeTypes.SysLambda;
        }


        /// <summary>
        /// The function expression.
        /// </summary>
        public FunctionExpr Expr;


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitLambda(this);
        }
    }
}
