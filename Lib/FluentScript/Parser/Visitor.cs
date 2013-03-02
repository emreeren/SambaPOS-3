﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Helpers;
using ComLib.Lang.Plugins;
// </lang:using>


namespace ComLib.Lang.Parsing
{
    /// <summary>
    /// Class that visits each ast node in the trees.
    /// </summary>
    public class AstVisitor : IAstVisitor
    {
        /// <summary>
        /// Callback 
        /// </summary>
        private Action<AstNode> _callBackOnNodeStart;
        private Action<AstNode> _callBackOnNodeEnd;


        /// <summary>
        /// Initialize
        /// </summary>
        public AstVisitor()
        {
            _callBackOnNodeStart = null;
            _callBackOnNodeEnd = null;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public AstVisitor(Action<AstNode> callBackOnNodeStart, Action<AstNode> callBackOnNodeEnd)
        {
            _callBackOnNodeStart = callBackOnNodeStart;
            _callBackOnNodeEnd = callBackOnNodeEnd;
        }


        /// <summary>
        /// Visits each statement
        /// </summary>
        /// <param name="stmts"></param>
        public object Visit(List<Expr> stmts)
        {
            foreach (var stmt in stmts)
            {
                stmt.Visit(this);
            }
            return null;
        }


        /// <summary>
        /// Visit the statement
        /// </summary>
        /// <param name="exp"></param>
        public object VisitExpr( Expr exp)
        {
            if (exp.IsNodeType(NodeTypes.SysAssign))
                VisitAssign(exp as AssignExpr);

            if (exp.IsNodeType(NodeTypes.SysAssignMulti))
                VisitAssignMulti(exp as AssignMultiExpr);

            else if (exp.IsNodeType(NodeTypes.SysFor))
                VisitFor(exp as ForExpr);

            else if (exp.IsNodeType(NodeTypes.SysForEach))
                VisitForEach(exp as ForEachExpr);

            else if (exp.IsNodeType(NodeTypes.SysIf))
                VisitIf(exp as IfExpr);

            else if (exp.IsNodeType(NodeTypes.SysTryCatch))
                VisitTryCatch(exp as TryCatchExpr);

            else if (exp.IsNodeType(NodeTypes.SysWhile))
                VisitWhile(exp as WhileExpr);

            else if (exp.IsNodeType(NodeTypes.SysBinary))
                VisitBinary(exp as BinaryExpr);

            else if (exp.IsNodeType(NodeTypes.SysCompare))
                VisitCompare(exp as CompareExpr);

            else if (exp.IsNodeType(NodeTypes.SysCondition))
                VisitCondition(exp as ConditionExpr);

            else if (exp.IsNodeType(NodeTypes.SysFunctionDeclare))
                VisitFunctionDeclare(exp as FunctionDeclareExpr);

            else if (exp.IsNodeType(NodeTypes.SysFunctionCall))
                VisitFunctionCall(exp as FunctionCallExpr);

            return null;
        }


        /// <summary>
        /// Visits the var statement tree.
        /// </summary>
        /// <param name="assignExpr"></param>
        public object VisitAssignMulti(AssignMultiExpr assignExpr)
        {
            _callBackOnNodeStart(assignExpr);
            foreach (var decl in assignExpr.Assignments)
            {
                VisitAssign(decl);
            }
            return null;
        }


        public object VisitArray(ArrayExpr expr)
        {
            return null;
        }


        public object VisitBlock(BlockExpr expr)
        {            
            foreach (var stmt in expr.Statements)
            {
                stmt.Visit(this);
            }
            return null;
        }


        /// <summary>
        /// Visits the var statement tree.
        /// </summary>
        /// <param name="assignExpr"></param>
        public object VisitAssign(AssignExpr expr)
        {
            _callBackOnNodeStart(expr);
            expr.VarExp.Visit(this);
            if (expr.ValueExp != null)
                expr.ValueExp.Visit(this);
            return null;
        }


        /// <summary>
        /// Visits the var statement tree.
        /// </summary>
        /// <param name="assignExpr"></param>
        public object VisitBreak(BreakExpr expr)
        {
            _callBackOnNodeStart(expr);
            return null;
        }


        /// <summary>
        /// Visits the var statement tree.
        /// </summary>
        /// <param name="assignExpr"></param>
        public object VisitContinue(ContinueExpr expr)
        {
            _callBackOnNodeStart(expr);
            return null;
        }


        /// <summary>
        /// Visits the for statement tree.
        /// </summary>
        /// <param name="forExpr"></param>
        public object VisitFor(ForExpr expr)
        {
            _callBackOnNodeStart(expr);
            expr.Start.Visit(this);
            expr.Condition.Visit(this);
            expr.Increment.Visit(this);
            foreach (var stmt in expr.Statements)
            {
                stmt.Visit(this);
            }
            return null;
        }


        /// <summary>
        /// Visits the for each statement tree.
        /// </summary>
        /// <param name="forExpr"></param>
        public object VisitForEach(ForEachExpr expr)
        {
            _callBackOnNodeStart(expr);
            expr.Condition.Visit(this);
            foreach (var stmt in expr.Statements)
            {
                stmt.Visit(this);
            }
            return null;
        }


        /// <summary>
        /// Visits the if statement tree.
        /// </summary>
        /// <param name="ifExpr"></param>
        public object VisitIf(IfExpr expr)
        {
            _callBackOnNodeStart(expr);
            expr.Condition.Visit(this);
            foreach (var stmt in expr.Statements)
            {
                stmt.Visit(this);
            }
            if (expr.Else != null)
                expr.Else.Visit(this);
            return null;
        }


        public object VisitLambda(LambdaExpr expr)
        {
            return null;
        }


        /// <summary>
        /// Visits the try statement tree.
        /// </summary>
        /// <param name="tryExpr"></param>
        public object VisitReturn(ReturnExpr expr)
        {
            _callBackOnNodeStart(expr);
            return null;
        }


        /// <summary>
        /// Visits the try statement tree.
        /// </summary>
        /// <param name="tryExpr"></param>
        public object VisitThrow(ThrowExpr expr)
        {
            _callBackOnNodeStart(expr);
            return null;
        }


        /// <summary>
        /// Visits the try statement tree.
        /// </summary>
        /// <param name="tryExpr"></param>
        public object VisitTryCatch(TryCatchExpr expr)
        {
            _callBackOnNodeStart(expr);
            foreach (var stmt in expr.Statements)
            {
                stmt.Visit(this);
            }
            foreach (var stmt in expr.Catch.Statements)
            {
                stmt.Visit(this);
            }
            return null;
        }


        /// <summary>
        /// Visits the while statement tree.
        /// </summary>
        /// <param name="whileExpr"></param>
        public object VisitWhile(WhileExpr expr)
        {
            _callBackOnNodeStart(expr);
            expr.Condition.Visit(this);
            foreach (var stmt in expr.Statements)
            {
                stmt.Visit(this);
            }
            return null;
        }


        /// <summary>
        /// Visits the binary expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitBinary(BinaryExpr exp)
        {
            _callBackOnNodeStart(exp);
            _callBackOnNodeStart(exp.Left);
            _callBackOnNodeStart(exp.Right);
            return null;
        }


        /// <summary>
        /// Visits the compare expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitCompare(CompareExpr exp)
        {
            _callBackOnNodeStart(exp);
            _callBackOnNodeStart(exp.Left);
            _callBackOnNodeStart(exp.Right);
            return null;
        }


        /// <summary>
        /// Visits the condition expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitCondition(ConditionExpr exp)
        {
            _callBackOnNodeStart(exp);
            _callBackOnNodeStart(exp.Left);
            _callBackOnNodeStart(exp.Right);
            return null;
        }


        /// <summary>
        /// Visits the condition expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitConstant(ConstantExpr exp)
        {
            return null;
        }


        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitFunctionDeclare(FunctionDeclareExpr exp)
        {
            _callBackOnNodeStart(exp);
            for(var ndx = 0; ndx < exp.Function.Statements.Count; ndx++)
            {
                var stmt = exp.Function.Statements[ndx];
                stmt.Visit(this);
            }
            _callBackOnNodeEnd(exp);
            return null;
        }


        public object VisitFunction(FunctionExpr expr)
        {
            return null;
        }
        
        
        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitFunctionCall(FunctionCallExpr exp)
        {
            _callBackOnNodeStart(exp);
            foreach (var paramExp in exp.ParamListExpressions)
                _callBackOnNodeStart(paramExp);
            _callBackOnNodeEnd(exp);
            return null;
        }


        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitIndex(IndexExpr exp)
        {
            _callBackOnNodeStart(exp);            
            _callBackOnNodeStart(exp.VarExp);
            _callBackOnNodeStart(exp.IndexExp);
            return null;
        }


        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitInterpolated(InterpolatedExpr exp)
        {
            return null;
        }


        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitMap(MapExpr exp)
        {
            return null;
        }


        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitMemberAccess(MemberAccessExpr exp)
        {
            return null;
        }


        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitNamedParameter(NamedParameterExpr exp)
        {
            return null;
        }

        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitNew(NewExpr exp)
        {
            return null;
        }


        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public object VisitParameter(ParameterExpr exp)
        {
            return null;
        }


        public object VisitUnary(UnaryExpr expr)
        {
            return null;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public object VisitNegate(NegateExpr expr)
        {
            return null;
        }


        public object VisitVariable(VariableExpr expr)
        {
            return null;
        }


        public void VisitBlockEnter(Expr expr)
        {
        }


        public void VisitBlockExit(Expr expr)
        {
        }
    }
}
