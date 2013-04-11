using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>


namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper class
    /// </summary>
    public class LangHelper
    {
        /// <summary>
        /// Converts a list of items to a dictionary with the items.
        /// </summary>
        /// <typeparam name="T">Type of items to use.</typeparam>
        /// <param name="items">List of items.</param>
        /// <returns>Converted list as dictionary.</returns>
        public static IDictionary<T, T> ToDictionary<T>(IList<T> items)
        {
            var dict = new Dictionary<T, T>();
            foreach (T item in items)
            {
                dict[item] = item;
            }
            return dict;
        }


        /// <summary>
        /// Converts a list of items to a dictionary with the items.
        /// </summary>
        /// <typeparam name="T">Type of items to use.</typeparam>
        /// <param name="items">List of items.</param>
        /// <returns>Converted list as dictionary.</returns>
        public static IDictionary<T, T> ToDictionaryFiltered<T>(IList<T> items )
        {
            var dict = new Dictionary<T, T>();
            foreach (T item in items)
            {
                dict[item] = item;
            }
            return dict;
        }


        /// <summary>
        /// Executes the statements.
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="parent"></param>
        public static void Evaluate(List<Expr> statements, AstNode parent, IAstVisitor visitor)
        {
            if (statements != null && statements.Count > 0)
            {
                foreach (var stmt in statements)
                {
                    stmt.Evaluate(visitor);
                }
            }
        }


        /// <summary>
        /// The shunting yard algorithm that processes a postfix list of expressions/operators.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="parser"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static Expr ProcessShuntingYardList(Context context, Parser.Parser parser, List<object> stack)
        {
            int index = 0;
            Expr finalExp = null;

            // Shunting yard algorithm handles POSTFIX operations.
            while (index < stack.Count && stack.Count > 0)
            {
                // Keep moving forward to the first operator * - + / && that is found  
                // This is a postfix algorithm so it works by creating an expression
                // from the last 2 items behind an operator.
                if (!(stack[index] is TokenData))
                {
                    index++;
                    continue;
                }

                // At this point... we hit an operator 
                // So get the last 2 items on the stack ( they have to be expressions )
                // left  is 2 behind current position
                // right is 1 behind current position
                var left = stack[index - 2] as Expr;
                var right = stack[index - 1] as Expr;
                TokenData tdata = stack[index] as TokenData;
                Token top = tdata.Token;
                Operator op = Operators.ToOp(top.Text);
                Expr exp = null;

                if (Operators.IsMath(op))
                    exp = Exprs.Binary(left, op, right, tdata);
                else if (Operators.IsConditional(op))
                    exp = Exprs.Condition(left, op, right, tdata);
                else if (Operators.IsCompare(op))
                    exp = Exprs.Compare(left, op, right, tdata);

                parser.SetupContext(exp, tdata);
                stack.RemoveRange(index - 2, 2);
                index = index - 2;
                stack[index] = exp;
                index++;

            }
            finalExp = stack[0] as Expr;
            return finalExp;
        }


        /// <summary>
        /// Executes an action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="exceptionMessageFetcher"></param>
        public static RunResult Execute(Action action, Func<string> exceptionMessageFetcher = null)
        {
            DateTime start = DateTime.Now;
            bool success = true;
            string message = string.Empty;
            Exception scriptError = null;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                
                success = false;
                if (ex is LangException)
                {
                    LangException lex = ex as LangException;
                    const string langerror = "{0} : {1} at line : {2}, position: {3}";
                    message = string.Format(langerror, lex.Error.ErrorType, lex.Message, lex.Error.Line, lex.Error.Column);
                }
                else message = ex.Message;

                scriptError = ex;
                if (exceptionMessageFetcher != null)
                    message += exceptionMessageFetcher();
            }
            DateTime end = DateTime.Now;
            var runResult = new RunResult(start, end, success, message);
            runResult.Ex = scriptError;
            return runResult;
        }
    }
}
