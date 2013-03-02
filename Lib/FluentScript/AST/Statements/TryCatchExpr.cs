
namespace ComLib.Lang.AST
{

    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class TryCatchExpr : BlockExpr
    {
        /// <summary>
        /// Create new instance
        /// </summary>
        public TryCatchExpr()
        {
            this.Nodetype = NodeTypes.SysTryCatch;
            InitBoundary(true, "}");
            Catch = new BlockExpr();
        }


        /// <summary>
        /// Name for the error in the catch clause.
        /// </summary>
        public string ErrorName;


        /// <summary>
        /// Else statement.
        /// </summary>
        public BlockExpr Catch;


        /// <summary>
        /// Disable management of memory scope by baseclass
        /// </summary>
        public override void OnBlockEnter()
        {
            //base.OnBlockEnter();
        }


        /// <summary>
        /// Disable management of memory scope by baseclass
        /// </summary>
        public override void OnBlockExit()
        {
            //base.OnBlockExit();
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitTryCatch(this);
        }
    }    
}
