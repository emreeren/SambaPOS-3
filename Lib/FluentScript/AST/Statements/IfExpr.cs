
namespace ComLib.Lang.AST
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class IfExpr : ConditionalBlockExpr
    {
        /// <summary>
        /// Create new instance
        /// </summary>
        public IfExpr() : base(null, null)
        {
            this.Nodetype = NodeTypes.SysIf;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="condition"></param>
        public IfExpr(Expr condition)
            : base(condition, null)
        {
            this.Nodetype = NodeTypes.SysIf;
            InitBoundary(true, "}");            
        }


        /// <summary>
        /// Else statement.
        /// </summary>
        public BlockExpr Else;



        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitIf(this);
        }
    }    
}
