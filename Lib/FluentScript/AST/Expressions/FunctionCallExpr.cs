using System;
using System.Collections.Generic;
using System.Collections;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.AST
{        
    /// <summary>
    /// Function call expression data.
    /// </summary>
    public class FunctionCallExpr : Expr, IParameterExpression
    {
        /// <summary>
        /// Function call expression
        /// </summary>
        public FunctionCallExpr()
        {
            this.Nodetype = NodeTypes.SysFunctionCall;
            this.InitBoundary(true, ")");
            this.ParamList = new List<object>();
            this.ParamListExpressions = new List<Expr>();
        } 


        /// <summary>
        /// Expression represnting the name of the function call.
        /// </summary>
        public Expr NameExp;


        /// <summary>
        /// List of expressions.
        /// </summary>
        public List<Expr> ParamListExpressions { get; set; }


        /// <summary>
        /// List of arguments.
        /// </summary>
        public List<object> ParamList { get; set; }


        /// <summary>
        /// Arguments to the function.
        /// </summary>
        public IDictionary ParamMap;


        /// <summary>
        /// The function expression.
        /// </summary>
        public FunctionExpr Function;


        /// <summary>
        /// Whether or not this is a method call or a member access.
        /// </summary>
        public bool IsScopeVariable { get; set; }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitFunctionCall(this);
        }

        /// <summary>
        /// Returns the fully qualified name of this node.
        /// </summary>
        /// <returns></returns>
        public override string ToQualifiedName()
        {
            if (NameExp != null)
                return NameExp.ToQualifiedName();
            return string.Empty;
        }
    }
}
