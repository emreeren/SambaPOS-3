using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class BinaryExpr : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="left">Left hand expression</param>
        /// <param name="op">Operator</param>
        /// <param name="right">Right expression</param>
        public BinaryExpr(Expr left, Operator op, Expr right)
        {
            Left = left;
            Right = right;
            Op = op;
        }


        /// <summary>
        /// Left hand expression
        /// </summary>
        public Expr Left;


        /// <summary>
        /// Operator * - / + % 
        /// </summary>
        public Operator Op;


        /// <summary>
        /// Right hand expression
        /// </summary>
        public Expr Right;


        /// <summary>
        /// Evaluate * / + - % 
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            // Validate
            object result = 0;
            object left = Left.Evaluate();
            object right = Right.Evaluate();
            
            if (left is int || left is long)
                left = Convert.ToDouble(left);
            if (right is int || left is long)
                right = Convert.ToDouble(right);

            // Both numbers
            if (left is double && right is double)
            {
                result = EvalNumbers((double)left, (double)right, Op);
            }
            // Both times
            else if (left is TimeSpan && right is TimeSpan)
            {
                result = EvalTimes((TimeSpan)left, (TimeSpan)right, Op);
            }
            // Both dates
            else if (left is DateTime && right is DateTime)
            {
                result = EvalDates((DateTime)left, (DateTime)right, Op);
            }
            else if (left is int && right is int)
            {
                result = EvalNumbers(Convert.ToDouble(left), Convert.ToDouble(right), Op);
            }
            // Both strings.
            else if (left is string && right is string)
            {
                string strleft = left.ToString();
                string strright = right.ToString();

                // Check string limit.
                Ctx.Limits.CheckStringLength(this, strleft, strright);

                result = strleft + strright;
            }
            // Double and Bool
            else if (left is double && right is bool)
            {
                bool r = (bool)right;
                double rval = r ? 1 : 0;
                result = EvalNumbers((double)left, rval, Op);
            }
            // Bool Double
            else if (left is bool && right is double)
            {
                bool l = (bool)left;
                double lval = l ? 1 : 0;
                result = EvalNumbers(lval, (double)right, Op);
            }
            // Append as strings.
            else if (left is string && right is bool)
            {
                result = left.ToString() + right.ToString().ToLower();
            }
            // Append as strings.
            else if (left is bool && right is string)
            {
                result = left.ToString().ToLower() + right.ToString();
            }
            else if (left is LUnit && right is LUnit)
            {
                result = EvalUnits((LUnit)left, (LUnit)right, Op, Ctx.Units);
            }
            else
            {
                result = left.ToString() + right.ToString();
            }
            return result;
        }


        private static double EvalNumbers(double left, double right, Operator op)
        {
            double result = 0;
            if (op == Operator.Multiply)
            {
                result = left * right;
            }
            else if (op == Operator.Divide)
            {
                result = left / right;
            }
            else if (op == Operator.Add)
            {
                result = left + right;
            }
            else if (op == Operator.Subtract)
            {
                result = left - right;
            }
            else if (op == Operator.Modulus)
            {
                result = left % right;
            }
            return result;
        }


        private static LUnit EvalUnits(LUnit left, LUnit right, Operator op, Units units)
        {
            double baseUnitsValue = 0;
            if (op == Operator.Multiply)
            {
                baseUnitsValue = left.BaseValue * right.BaseValue;
            }
            else if (op == Operator.Divide)
            {
                baseUnitsValue = left.BaseValue / right.BaseValue;
            }
            else if (op == Operator.Add)
            {
                baseUnitsValue = left.BaseValue + right.BaseValue;
            }
            else if (op == Operator.Subtract)
            {
                baseUnitsValue = left.BaseValue - right.BaseValue;
            }
            else if (op == Operator.Modulus)
            {
                baseUnitsValue = left.BaseValue % right.BaseValue;
            }

            var relativeValue = units.ConvertToRelativeValue(baseUnitsValue, left.SubGroup, null);
            var result = new LUnit() { BaseValue = baseUnitsValue, Group = left.Group, SubGroup = left.SubGroup, Value = relativeValue };
            return result;
        }


        private TimeSpan EvalTimes(TimeSpan left, TimeSpan right, Operator op)
        {
            if (op != Operator.Add && op != Operator.Subtract)
                throw BuildRunTimeException("Can only add/subtract times");

            TimeSpan result = TimeSpan.MinValue;
            if (op == Operator.Add)
            {
                result = left + right;
            }
            else if (op == Operator.Subtract)
            {
                result = left - right;
            }
            return result;
        }


        private TimeSpan EvalDates(DateTime left, DateTime right, Operator op)
        {
            if (op != Operator.Subtract)
                throw BuildRunTimeException("Can only subtract dates");

            TimeSpan result = TimeSpan.MinValue;
            result = left - right;
            return result;
        }
    }    
}
