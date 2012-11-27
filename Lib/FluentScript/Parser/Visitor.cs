using System;
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
    public class AstVisitor
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
        public void Visit(List<Expr> stmts)
        {
            foreach (var stmt in stmts)
            {
                Visit(stmt);
            }
        }


        /// <summary>
        /// Visit the statement
        /// </summary>
        /// <param name="exp"></param>
        public void Visit( Expr exp)
        {
            if (exp.IsNodeType(NodeTypes.SysAssign))
                VarSingle(exp as AssignExpr);

            if (exp.IsNodeType(NodeTypes.SysAssignMulti))
                VarMulti(exp as MultiAssignExpr);

            else if (exp.IsNodeType(NodeTypes.SysFor))
                For(exp as ForExpr);

            else if (exp.IsNodeType(NodeTypes.SysForEach))
                ForEach(exp as ForEachExpr);

            else if (exp.IsNodeType(NodeTypes.SysIf))
                If(exp as IfExpr);

            else if (exp.IsNodeType(NodeTypes.SysTryCatch))
                Try(exp as TryCatchExpr);

            else if (exp.IsNodeType(NodeTypes.SysWhile))
                While(exp as WhileExpr);

            else if (exp.IsNodeType(NodeTypes.SysBinary))
                Binary(exp as BinaryExpr);

            else if (exp.IsNodeType(NodeTypes.SysCompare))
                Compare(exp as CompareExpr);

            else if (exp.IsNodeType(NodeTypes.SysCondition))
                Condition(exp as ConditionExpr);

            else if (exp.IsNodeType(NodeTypes.SysFunctionDeclare))
                FunctionDeclare(exp as FuncDeclareExpr);

            else if (exp.IsNodeType(NodeTypes.SysFunctionCall))
                FunctionCall(exp as FunctionCallExpr);
        }


        /// <summary>
        /// Visits the var statement tree.
        /// </summary>
        /// <param name="assignExpr"></param>
        public void VarMulti(MultiAssignExpr assignExpr)
        {
            _callBackOnNodeStart(assignExpr);
            foreach (var decl in assignExpr._assignments)
            {
                VarSingle(decl);
            }
        }


        /// <summary>
        /// Visits the var statement tree.
        /// </summary>
        /// <param name="assignExpr"></param>
        public void VarSingle(AssignExpr assignExpr)
        {
            _callBackOnNodeStart(assignExpr);
            Visit(assignExpr.VarExp);
            Visit(assignExpr.ValueExp);
        }


        /// <summary>
        /// Visits the for statement tree.
        /// </summary>
        /// <param name="forExpr"></param>
        public void For(ForExpr forExpr)
        {
            _callBackOnNodeStart(forExpr);
            Visit(forExpr.Start);
            Visit(forExpr.Condition);
            Visit(forExpr.Increment);
            foreach (var stmt in forExpr.Statements)
            {
                Visit(stmt);
            }
        }


        /// <summary>
        /// Visits the for each statement tree.
        /// </summary>
        /// <param name="forExpr"></param>
        public void ForEach(ForEachExpr forExpr)
        {
            _callBackOnNodeStart(forExpr);
            Visit(forExpr.Condition);
            foreach (var stmt in forExpr.Statements)
            {
                Visit(stmt);
            }
        }


        /// <summary>
        /// Visits the if statement tree.
        /// </summary>
        /// <param name="ifExpr"></param>
        public void If(IfExpr ifExpr)
        {
            _callBackOnNodeStart(ifExpr);
            Visit(ifExpr.Condition);
            foreach (var stmt in ifExpr.Statements)
            {
                Visit(stmt);
            }
            Visit(ifExpr.Else);
        }


        /// <summary>
        /// Visits the try statement tree.
        /// </summary>
        /// <param name="tryExpr"></param>
        public void Try(TryCatchExpr tryExpr)
        {
            _callBackOnNodeStart(tryExpr);
            foreach (var stmt in tryExpr.Statements)
            {
                Visit(stmt);
            }
            Visit(tryExpr.Catch);
        }


        /// <summary>
        /// Visits the while statement tree.
        /// </summary>
        /// <param name="whileExpr"></param>
        public void While(WhileExpr whileExpr)
        {
            _callBackOnNodeStart(whileExpr);
            Visit(whileExpr.Condition);
            foreach (var stmt in whileExpr.Statements)
            {
                Visit(stmt);
            }
        }


        /// <summary>
        /// Visits the binary expression tree
        /// </summary>
        /// <param name="exp"></param>
        public void Binary(BinaryExpr exp)
        {
            _callBackOnNodeStart(exp);
            _callBackOnNodeStart(exp.Left);
            _callBackOnNodeStart(exp.Right);
        }


        /// <summary>
        /// Visits the compare expression tree
        /// </summary>
        /// <param name="exp"></param>
        public void Compare(CompareExpr exp)
        {
            _callBackOnNodeStart(exp);
            _callBackOnNodeStart(exp.Left);
            _callBackOnNodeStart(exp.Right);
        }


        /// <summary>
        /// Visits the condition expression tree
        /// </summary>
        /// <param name="exp"></param>
        public void Condition(ConditionExpr exp)
        {
            _callBackOnNodeStart(exp);
            _callBackOnNodeStart(exp.Left);
            _callBackOnNodeStart(exp.Right);
        }


        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public void FunctionDeclare(FuncDeclareExpr exp)
        {
            _callBackOnNodeStart(exp);
            for(var ndx = 0; ndx < exp.Function.Statements.Count; ndx++)
            {
                var stmt = exp.Function.Statements[ndx];
                Visit(stmt);
            }
            _callBackOnNodeEnd(exp);
        }
        
        
        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public void FunctionCall(FunctionCallExpr exp)
        {
            _callBackOnNodeStart(exp);
            foreach (var paramExp in exp.ParamListExpressions)
                _callBackOnNodeStart(paramExp);
            _callBackOnNodeEnd(exp);
        }
    }
}
