
namespace ComLib.Lang.AST
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class WhileExpr : ConditionalBlockExpr, ILoop
    {
        /// <summary>
        /// Whether or not the break the loop
        /// </summary>
        public bool DoBreakLoop { get; set; }


        /// <summary>
        /// Whether or not to continue the loop
        /// </summary>
        public bool DoContinueLoop { get; set; }


        /// <summary>
        /// Whether or not to continue running the loop
        /// </summary>
        public bool DoContinueRunning { get; set; }


        /// <summary>
        /// Create new instance/
        /// </summary>
        public WhileExpr() : base(null, null) { }


        /// <summary>
        /// Create new instance with condition
        /// </summary>
        /// <param name="condition"></param>
        public WhileExpr(Expr condition)
            : base(condition, null)
        {
            this.Nodetype = NodeTypes.SysWhile;
            InitBoundary(true, "}");
        }

        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitWhile(this);
        }
    }    
}
