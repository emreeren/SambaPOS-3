using System.Collections.Generic;

namespace ComLib.Lang.AST
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
