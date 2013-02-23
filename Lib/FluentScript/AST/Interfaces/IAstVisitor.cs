using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLib.Lang.AST
{
    public interface IAstVisitor
    {
        object VisitExpr            (   Expr                expr);
        object VisitBreak	        (   BreakExpr           expr );
        object VisitContinue        (   ContinueExpr        expr );
        object VisitForEach	        (   ForEachExpr         expr );
        object VisitFor	            (   ForExpr             expr );
        object VisitFunctionDeclare	(   FunctionDeclareExpr expr );
        object VisitIf	            (   IfExpr              expr );
        object VisitLambda	        (   LambdaExpr          expr );
        object VisitReturn	        (   ReturnExpr          expr );
        object VisitThrow	        (   ThrowExpr           expr );
        object VisitTryCatch        (   TryCatchExpr        expr );
        object VisitWhile	        (   WhileExpr           expr );
        object VisitArray           (	ArrayExpr			expr );
        object VisitAssign			(	AssignExpr			expr );
        object VisitAssignMulti		(	AssignMultiExpr		expr );
        object VisitBinary			(	BinaryExpr			expr );
        object VisitBlock			(	BlockExpr			expr );
        object VisitCompare			(	CompareExpr			expr );
        object VisitCondition		(	ConditionExpr		expr );
        object VisitConstant		(	ConstantExpr		expr );
        object VisitFunctionCall	(	FunctionCallExpr	expr );
        object VisitFunction		(	FunctionExpr		expr );
        object VisitIndex			(	IndexExpr			expr );
        object VisitInterpolated	(	InterpolatedExpr	expr );
        object VisitMap				(	MapExpr				expr );
        object VisitMemberAccess	(	MemberAccessExpr	expr );
        object VisitNamedParameter	(	NamedParameterExpr	expr );
        object VisitNegate			(	NegateExpr     	    expr );
        object VisitNew				(	NewExpr				expr );
        object VisitParameter		(	ParameterExpr		expr );
        object VisitUnary			(	UnaryExpr			expr );
        object VisitVariable		(	VariableExpr		expr );

        void VisitBlockEnter        (   Expr expr                );
        void VisitBlockExit         (   Expr expr                );
    }
}
