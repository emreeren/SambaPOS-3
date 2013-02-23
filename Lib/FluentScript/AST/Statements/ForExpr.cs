
namespace ComLib.Lang.AST
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class ForExpr : WhileExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public ForExpr()
            : this(null, null, null)
        {
            this.Nodetype = NodeTypes.SysFor;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="start">start expression</param>
        /// <param name="condition">condition for loop</param>
        /// <param name="inc">increment expression</param>
        public ForExpr(Expr start, Expr condition, Expr inc)
            : base(condition)
        {
            this.Nodetype = NodeTypes.SysFor;
            InitBoundary(true, "}");
            Init(start, condition, inc);
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="start">start expression</param>
        /// <param name="condition">condition for loop</param>
        /// <param name="inc">increment expression</param>
        public void Init(Expr start, Expr condition, Expr inc)
        {
            Start = start;
            Increment = inc;
            Condition = condition;
        }


        /// <summary>
        /// Start statement.
        /// </summary>
        public Expr Start;


        /// <summary>
        /// Increment statement.
        /// </summary>
        public Expr Increment;



        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitFor(this);
        }
    }
}
