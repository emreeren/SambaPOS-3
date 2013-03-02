using System;
using System.Collections.Generic;

using ComLib.Lang.Core;


namespace ComLib.Lang.AST
{
    /// ------------------------------------------------------------------------------------------------
    /// remarks: This file is auto-generated from the FSGrammar specification and should not be modified.
    /// summary: This file contains all the AST for expressions at the system level.
    ///			features like control-flow e..g if, while, for, try, break, continue, return etc.
    /// version: 0.9.8.10
    /// author:  kishore reddy
    /// date:	01/11/13 04:02:40 PM
    /// ------------------------------------------------------------------------------------------------


    /// <summary>1: AST class for ArrayExpr</summary>
    public class ArrayExpr : IndexableExpr
    {
        public ArrayExpr()
        {
            this.Nodetype = NodeTypes.SysArray;
        }


        public List<Expr> Exprs;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitArray(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitArray(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>2: AST class for AssignExpr</summary>
    public class AssignExpr : Expr
    {
        public AssignExpr()
        {
            this.Nodetype = NodeTypes.SysAssign;
        }


        public Expr VarExp;

        public Expr ValueExp;

        public bool IsDeclaration;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitAssign(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitAssign(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>3: AST class for AssignMultiExpr</summary>
    public class AssignMultiExpr : Expr
    {
        public AssignMultiExpr()
        {
            this.Nodetype = NodeTypes.SysAssignMulti;
        }


        public List<AssignExpr> Assignments;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitAssignMulti(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitAssignMulti(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>4: AST class for BinaryExpr</summary>
    public class BinaryExpr : Expr
    {
        public BinaryExpr()
        {
            this.Nodetype = NodeTypes.SysBinary;
        }
        public Expr Left;

        public Expr Right;

        public Operator Op;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitBinary(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitBinary(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>5: AST class for CompareExpr</summary>
    public class CompareExpr : Expr
    {
        public CompareExpr()
        {
            this.Nodetype = NodeTypes.SysCompare;
        }
        public Expr Left;

        public Expr Right;

        public Operator Op;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitCompare(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitCompare(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>6: AST class for ConditionExpr</summary>
    public class ConditionExpr : Expr
    {
        public ConditionExpr()
        {
            this.Nodetype = NodeTypes.SysCondition;
        }
        public Expr Left;

        public Expr Right;

        public Operator Op;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitCondition(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitCondition(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>7: AST class for ConstantExpr</summary>
    public class ConstantExpr : ValueExpr
    {
        public ConstantExpr()
        {
            this.Nodetype = NodeTypes.SysConstant;
        }


        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitConstant(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitConstant(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>8: AST class for FunctionCallExpr</summary>
    public class FunctionCallExpr : Expr, IParameterExpression
    {
        public FunctionCallExpr()
        {
            this.Nodetype = NodeTypes.SysFunctionCall;
        }


        public Expr NameExp;

        public List<Expr> ParamListExpressions { get; set; }

        public List<object> ParamList { get; set; }

        public FunctionExpr Function;

        public bool IsScopeVariable;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitFunctionCall(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitFunctionCall(this);
        }

        public override string ToQualifiedName()
        {
            return this.NameExp != null ? this.NameExp.ToQualifiedName() : "";
        }
    }


    /// <summary>9: AST class for FunctionExpr</summary>
    public class FunctionExpr : BlockExpr
    {
        public FunctionExpr()
        {
            this.Nodetype = NodeTypes.SysFunction;
        }


        public FunctionMetaData Meta;

        public long ExecutionCount;

        public long ErrorCount;

        public bool HasReturnValue;

        public object ReturnValue;

        public List<object> ArgumentValues;

        public bool ContinueRunning;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitFunction(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitFunction(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>10: AST class for IndexExpr</summary>
    public class IndexExpr : Expr
    {
        public IndexExpr()
        {
            this.Nodetype = NodeTypes.SysIndex;
        }


        public Expr IndexExp;

        public Expr VarExp;

        public bool IsAssignment;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitIndex(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitIndex(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>11: AST class for InterpolatedExpr</summary>
    public class InterpolatedExpr : Expr
    {
        public InterpolatedExpr()
        {
            this.Nodetype = NodeTypes.SysInterpolated;
        }


        public List<Expr> Expressions;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitInterpolated(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitInterpolated(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>12: AST class for MapExpr</summary>
    public class MapExpr : IndexableExpr
    {
        public MapExpr()
        {
            this.Nodetype = NodeTypes.SysMap;
        }


        public List<Tuple<string, Expr>> Expressions;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitMap(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitMap(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>13: AST class for MemberAccessExpr</summary>
    public class MemberAccessExpr : Expr
    {
        public MemberAccessExpr()
        {
            this.Nodetype = NodeTypes.SysMemberAccess;
        }


        public string MemberName;

        public Expr VarExp;

        public bool IsAssignment;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitMemberAccess(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitMemberAccess(this);
        }

        public override string ToQualifiedName()
        {
            return this.VarExp.ToQualifiedName() + "." + this.MemberName;
        }
    }


    /// <summary>14: AST class for NamedParameterExpr</summary>
    public class NamedParameterExpr : Expr
    {
        public NamedParameterExpr()
        {
            this.Nodetype = NodeTypes.SysNamedParameter;
        }


        public string Name;

        public Expr Value;

        public int Pos;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitNamedParameter(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitNamedParameter(this);
        }

        public override string ToQualifiedName()
        {
            return this.Name;
        }
    }


    /// <summary>15: AST class for NegateExpr</summary>
    public class NegateExpr : VariableExpr
    {
        public NegateExpr()
        {
            this.Nodetype = NodeTypes.SysNegate;
        }


        public Expr Expression;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitNegate(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitNegate(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>16: AST class for NewExpr</summary>
    public class NewExpr : Expr, IParameterExpression
    {
        public NewExpr()
        {
            this.Nodetype = NodeTypes.SysNew;
        }


        public string TypeName;

        public List<Expr> ParamListExpressions { get; set; }

        public List<object> ParamList { get; set; }

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitNew(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitNew(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>17: AST class for ParameterExpr</summary>
    public class ParameterExpr : Expr, IParameterExpression
    {
        public ParameterExpr()
        {
            this.Nodetype = NodeTypes.SysParameter;
        }


        public FunctionMetaData Meta;

        public List<Expr> ParamListExpressions { get; set; }

        public List<object> ParamList { get; set; }

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitParameter(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitParameter(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>18: AST class for UnaryExpr</summary>
    public class UnaryExpr : VariableExpr
    {
        public UnaryExpr()
        {
            this.Nodetype = NodeTypes.SysUnary;
        }


        public Operator Op;

        public Expr Expression;

        public double Increment;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitUnary(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitUnary(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>30: AST class for VariableExpr</summary>
    public class VariableExpr : Expr
    {
        public VariableExpr()
        {
            this.Nodetype = NodeTypes.SysVariable;
        }


        public string Name;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitVariable(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitVariable(this);
        }

        public override string ToQualifiedName()
        {
            return this.Name;
        }
    }
}