

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class NamedParameterExpr : Expr
    {
        /// <summary>
        /// The name of the expression.
        /// </summary>
        public string Name;


        /// <summary>
        /// The expression representing the value of the parameter..
        /// </summary>
        public Expr Value;


        /// <summary>
        /// Position of the named arg.
        /// </summary>
        public int Pos;


        /// <summary>
        /// Initialize
        /// </summary>
        public NamedParameterExpr() : this(null, null)
        {
            this.Nodetype = NodeTypes.SysNamedParameter;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">The expression representing the value of the parameter.</param>
        public NamedParameterExpr(string name, Expr value)
        {
            this.Nodetype = NodeTypes.SysNamedParameter;
            this.Name = name;
            this.Value = value;
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
            return visitor.VisitNamedParameter(this);
        }
    }
}
