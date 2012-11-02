using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Interface for a visitor that can walk the node
    /// </summary>
    public interface IAstVisitor
    {
        /// <summary>
        /// Visits the node.
        /// </summary>
        /// <param name="node"></param>
        void VisitNode(AstNode node);
    }


    /// <summary>
    /// Interface for AST nodes to implement if they want to manually handle 
    /// walking its AST node ( expression, statement )
    /// </summary>
    public interface IAstWalk
    {
        /// <summary>
        /// Accepts the visitor and calls visit on each of this instances child nodes.
        /// </summary>
        /// <param name="visitor"></param>
        void Accept(IAstVisitor visitor);
    }
}
