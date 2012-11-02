using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang.Helpers
{
    /// <summary>
    /// Helper class
    /// </summary>
    public class LangHelper
    {
        /// <summary>
        /// Whether or not the type supplied is a basic type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsBasicType(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            Type type = obj.GetType();
            if (type == typeof(int)) return true;
            if (type == typeof(long)) return true;
            if (type == typeof(double)) return true;
            if (type == typeof(bool)) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(DateTime)) return true;

            return false;
        }


        /// <summary>
        /// Converts each item in the parameters object array to an integer.
        /// </summary>
        /// <param name="parameters"></param>
        public static int[] ConvertToInts(object[] parameters)
        {
            // Convert all parameters to int            
            int[] args = new int[parameters.Length];
            for (int ndx = 0; ndx < parameters.Length; ndx++)
            {
                args[ndx] = Convert.ToInt32(parameters[ndx]);
            }
            return args;
        }


        /// <summary>
        /// Converts a list of items to a dictionary with the items.
        /// </summary>
        /// <typeparam name="T">Type of items to use.</typeparam>
        /// <param name="items">List of items.</param>
        /// <returns>Converted list as dictionary.</returns>
        public static IDictionary<T, T> ToDictionary<T>(IList<T> items)
        {
            IDictionary<T, T> dict = new Dictionary<T, T>();
            foreach (T item in items)
            {
                dict[item] = item;
            }
            return dict;
        }

        /*
        /// <summary>
        /// Executes the statements.
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="parent"></param>
        public static void Execute(List<Stmt> statements, Stmt parent)
        {
            if (statements != null && statements.Count > 0)
            {
                foreach (var stmt in statements)
                {
                    stmt.Execute();
                }
            }
        }
        */

        /// <summary>
        /// Executes the statements.
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="parent"></param>
        public static void Evaluate(List<Expr> statements, AstNode parent)
        {
            if (statements != null && statements.Count > 0)
            {
                foreach (var stmt in statements)
                {
                    stmt.Evaluate();
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
        public static Expr ProcessShuntingYardList(Context context, Parser parser, List<object> stack)
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
                    exp = new BinaryExpr(left, op, right);
                else if (Operators.IsConditional(op))
                    exp = new ConditionExpr(left, op, right);
                else if (Operators.IsCompare(op))
                    exp = new CompareExpr(left, op, right);

                exp.Ctx = context;
                parser.SetScriptPosition(exp, tdata);
                stack.RemoveRange(index - 2, 2);
                index = index - 2;
                stack[index] = exp;
                index++;

            }
            finalExp = stack[0] as Expr;
            return finalExp;
        }
    }
}
