
namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class VariableExpr : ValueExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public VariableExpr()
        {
            this.Nodetype = NodeTypes.SysVariable;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">Variable name</param>
        public VariableExpr(string name)
        {
            this.Nodetype = NodeTypes.SysVariable;
            this.Name = name;
        }


        /// <summary>
        /// Returns the fully qualified name of this node.
        /// </summary>
        /// <returns></returns>
        public override string ToQualifiedName()
        {
            return this.Name;
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitVariable(this);
        }
    }
}
