using System;
using System.Collections.Generic;

// <lang:using>
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// New instance creation.
    /// </summary>
    public class NewExpr : Expr, IParameterExpression
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public NewExpr()
        {
            this.Nodetype = NodeTypes.SysNew;
            this.InitBoundary(true, ")");
            this.ParamList = new List<object>();
            this.ParamListExpressions = new List<Expr>();
        }


        /// <summary>
        /// Name of 
        /// </summary>
        public string TypeName { get; set; }



        /// <summary>
        /// List of expressions.
        /// </summary>
        public List<Expr> ParamListExpressions { get; set; }


        /// <summary>
        /// List of arguments.
        /// </summary>
        public List<object> ParamList { get; set; }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitNew(this);
        }
    }
}
