using System;

// <lang:using>
using ComLib.Lang.Core;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class BinaryExpr : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="left">Left hand expression</param>
        /// <param name="op">Operator</param>
        /// <param name="right">Right expression</param>
        public BinaryExpr(Expr left, Operator op, Expr right)
        {
            this.Nodetype = NodeTypes.SysBinary;
            this.Left = left;
            this.Right = right;
            this.AddChild(left);
            this.AddChild(right);
            this.Op = op;
        }


        /// <summary>
        /// Left hand expression
        /// </summary>
        public Expr Left;


        /// <summary>
        /// Operator * - / + % 
        /// </summary>
        public Operator Op;


        /// <summary>
        /// Right hand expression
        /// </summary>
        public Expr Right;


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitBinary(this);
        }
    }    
}
