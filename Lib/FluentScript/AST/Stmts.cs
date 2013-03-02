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
    /// date:	12/23/12 03:06:25 PM
    /// ------------------------------------------------------------------------------------------------


    /// <summary>18: AST class for BreakExpr</summary>
    public class BreakExpr : Expr
    {
        public BreakExpr()
        {
            this.Nodetype = NodeTypes.SysBreak;
        }


        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitBreak(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitBreak(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>19: AST class for ContinueExpr</summary>
    public class ContinueExpr : Expr
    {
        public ContinueExpr()
        {
            this.Nodetype = NodeTypes.SysContinue;
        }


        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitContinue(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitContinue(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>20: AST class for ForEachExpr</summary>
    public class ForEachExpr : WhileExpr
    {
        public ForEachExpr()
        {
            this.Nodetype = NodeTypes.SysForEach;
        }


        public string VarName;

        public string SourceName;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitForEach(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitForEach(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>21: AST class for ForExpr</summary>
    public class ForExpr : WhileExpr
    {
        public ForExpr()
        {
            this.Nodetype = NodeTypes.SysFor;
        }


        public Expr Start;

        public Expr Increment;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitFor(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitFor(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>22: AST class for FunctionDeclareExpr</summary>
    public class FunctionDeclareExpr : Expr
    {
        public FunctionDeclareExpr()
        {
            this.Nodetype = NodeTypes.SysFunctionDeclare;
        }


        public FunctionExpr Function;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitFunctionDeclare(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitFunctionDeclare(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>23: AST class for IfExpr</summary>
    public class IfExpr : ConditionalBlockExpr
    {
        public IfExpr()
        {
            this.Nodetype = NodeTypes.SysIf;
        }


        public BlockExpr Else;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitIf(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitIf(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>24: AST class for LambdaExpr</summary>
    public class LambdaExpr : Expr
    {
        public LambdaExpr()
        {
            this.Nodetype = NodeTypes.SysLambda;
        }


        public FunctionExpr Expr;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitLambda(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitLambda(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>25: AST class for ReturnExpr</summary>
    public class ReturnExpr : Expr
    {
        public ReturnExpr()
        {
            this.Nodetype = NodeTypes.SysReturn;
        }


        public Expr Exp;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitReturn(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitReturn(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>26: AST class for ThrowExpr</summary>
    public class ThrowExpr : Expr
    {
        public ThrowExpr()
        {
            this.Nodetype = NodeTypes.SysThrow;
        }


        public Expr Exp;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitThrow(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitThrow(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>27: AST class for TryCatchExpr</summary>
    public class TryCatchExpr : Expr, IBlockExpr
    {
        public TryCatchExpr()
        {
            this.Nodetype = NodeTypes.SysTryCatch;
        }


        public string ErrorName;

        public List<Expr> Statements { get; set; }

        public BlockExpr Catch;

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitTryCatch(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitTryCatch(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }


    /// <summary>28: AST class for WhileExpr</summary>
    public class WhileExpr : ConditionalBlockExpr, ILoop
    {
        public WhileExpr()
        {
            this.Nodetype = NodeTypes.SysWhile;
        }


        public bool DoBreakLoop { get; set; }

        public bool DoContinueLoop { get; set; }

        public bool DoContinueRunning { get; set; }

        public override object DoVisit(IAstVisitor visitor)
        {
            return visitor.VisitWhile(this);
        }

        public override object DoEvaluate(IAstVisitor visitor)
        {
            return visitor.VisitWhile(this);
        }

        public override string ToQualifiedName()
        {
            return string.Empty;
        }
    }
}