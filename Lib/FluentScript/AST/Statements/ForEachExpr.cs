
namespace ComLib.Lang.AST
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class ForEachExpr : WhileExpr
    {
        public string VarName;
        public string SourceName;


        /// <summary>
        /// Initialize using the variable names.
        /// </summary>
        /// <param name="varname">Name of the variable in the loop</param>
        /// <param name="sourceName">Name of the variable containing the items to loop through.</param>
        public ForEachExpr(string varname, string sourceName)
            : base(null)
        {
            this.Nodetype = NodeTypes.SysForEach;
            this.VarName = varname;
            this.SourceName = sourceName;
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitForEach(this);
        }
    }    
}
