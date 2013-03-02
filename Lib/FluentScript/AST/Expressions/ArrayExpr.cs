using System;
using System.Collections.Generic;
using System.Collections;


namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class ArrayExpr : IndexableExpr
    {
       public List<Expr> Expressions;


        /// <summary>
        /// Initialize
        /// </summary>
        public ArrayExpr(List<Expr> expressions)
        {
            this.Nodetype = NodeTypes.SysArray;
            // Used for lists/arrays
            this.InitBoundary(true, "]");
            this.Expressions = expressions;
        }


        /// <summary>
        /// Whether or not this is of the node type supplied.
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public override bool IsNodeType(string nodeType)
        {
            if (nodeType == NodeTypes.SysDataType)
                return true;
            return base.IsNodeType(nodeType);
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitArray(this);
        }
    }
}
