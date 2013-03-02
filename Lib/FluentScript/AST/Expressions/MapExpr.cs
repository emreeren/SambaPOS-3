using System;
using System.Collections.Generic;

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class MapExpr : IndexableExpr
    {
        public List<Tuple<string, Expr>> Expressions;
        

        /// <summary>
        /// Initialize
        /// </summary>
        public MapExpr(List<Tuple<string, Expr>> expressions)
        {
            this.Nodetype = NodeTypes.SysMap;
            // Used for maps
            this.InitBoundary(true, "}");
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
            return visitor.VisitMap(this);
        }
    }
}
