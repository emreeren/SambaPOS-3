
namespace ComLib.Lang.AST
{
    /// <summary>
    /// Represents a function declaration
    /// </summary>
    public class FunctionDeclareExpr : BlockExpr
    {
        private FunctionExpr _function = new FunctionExpr();


        /// <summary>
        /// Initialize
        /// </summary>
        public FunctionDeclareExpr()
        {
            this.Nodetype = NodeTypes.SysFunctionDeclare;
        }


        /// <summary>
        /// Function 
        /// </summary>
        public FunctionExpr Function
        {
            get { return _function; }
        }


        /// <summary>
        /// String representation
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="incrementTab"></param>
        /// <param name="includeNewLine"></param>
        /// <returns></returns>
        public override string AsString(string tab = "", bool incrementTab = false, bool includeNewLine = true)
        {
            return _function.AsString(tab, incrementTab, includeNewLine);
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitFunctionDeclare(this);
        }
    }
}
