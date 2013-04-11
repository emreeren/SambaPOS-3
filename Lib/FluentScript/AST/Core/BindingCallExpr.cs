using System.Collections.Generic;
using Fluentscript.Lib.AST.Interfaces;

namespace Fluentscript.Lib.AST.Core
{
    /// <summary>10: AST class for FunctionCallExpr</summary>
    public class BindingCallExpr : Expr, IParameterExpression
    {
        public BindingCallExpr()
        {
            this.Nodetype = NodeTypes.SysFunctionCall;
        }

        public string Name;

        public string FullName;

        public List<Expr> ParamListExpressions { get; set; }

        public List<object> ParamList { get; set; }

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitBindingCall(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitBindingCall(this);
        }

        public override string ToQualifiedName()
        {
            return this.FullName;
        }
    }
}
