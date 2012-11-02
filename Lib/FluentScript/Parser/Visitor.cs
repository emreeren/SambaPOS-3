using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Class that visits each ast node in the trees.
    /// </summary>
    public class AstVisitor
    {
        /// <summary>
        /// Callback 
        /// </summary>
        private Action<AstNode> _callBack;


        /// <summary>
        /// Initialize
        /// </summary>
        public AstVisitor()
        {
            _callBack = null;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public AstVisitor(Action<AstNode> callBack)
        {
            _callBack = callBack;
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
            if (exp is AssignExpr)
                Var(exp as AssignExpr);

            else if (exp is ForExpr)
                For(exp as ForExpr);

            else if (exp is ForEachExpr)
                ForEach(exp as ForEachExpr);

            else if (exp is IfExpr)
                If(exp as IfExpr);

            else if (exp is TryCatchExpr)
                Try(exp as TryCatchExpr);

            else if (exp is WhileExpr)
                While(exp as WhileExpr);

            else if (exp is BinaryExpr)
                Binary(exp as BinaryExpr);

            else if (exp is CompareExpr)
                Compare(exp as CompareExpr);

            else if (exp is ConditionExpr)
                Condition(exp as ConditionExpr);

            else if (exp is FunctionCallExpr)
                FunctionCall(exp as FunctionCallExpr);
        }


        /// <summary>
        /// Visits the var statement tree.
        /// </summary>
        /// <param name="assignExpr"></param>
        public void Var(AssignExpr assignExpr)
        {
            _callBack(assignExpr);
            foreach (var decl in assignExpr._declarations)
            {
                Visit(decl.Item1);
                Visit(decl.Item2);
            }
        }


        /// <summary>
        /// Visits the for statement tree.
        /// </summary>
        /// <param name="forExpr"></param>
        public void For(ForExpr forExpr)
        {
            _callBack(forExpr);
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
            _callBack(forExpr);
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
            _callBack(ifExpr);
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
            _callBack(tryExpr);
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
            _callBack(whileExpr);
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
            _callBack(exp);
            _callBack(exp.Left);
            _callBack(exp.Right);
        }


        /// <summary>
        /// Visits the compare expression tree
        /// </summary>
        /// <param name="exp"></param>
        public void Compare(CompareExpr exp)
        {
            _callBack(exp);
            _callBack(exp.Left);
            _callBack(exp.Right);
        }


        /// <summary>
        /// Visits the condition expression tree
        /// </summary>
        /// <param name="exp"></param>
        public void Condition(ConditionExpr exp)
        {
            _callBack(exp);
            _callBack(exp.Left);
            _callBack(exp.Right);
        }


        /// <summary>
        /// Visits the function call expression tree
        /// </summary>
        /// <param name="exp"></param>
        public void FunctionCall(FunctionCallExpr exp)
        {
            _callBack(exp);
            foreach (var paramExp in exp.ParamListExpressions)
                _callBack(paramExp);
        }
    }
}
