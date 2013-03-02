using System;

// <lang:using>
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class ConstantExpr : ValueExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public ConstantExpr(object val)
        {
            this.Nodetype = NodeTypes.SysConstant;
            this.Value = val;
            this.DataType = val.GetType();
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitConstant(this);
        }
    }
}
