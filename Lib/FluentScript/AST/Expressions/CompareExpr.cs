using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang
{
    /// <summary>
    /// Condition expression less, less than equal, more, more than equal etc.
    /// </summary>
    public class CompareExpr : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="left">Left hand expression</param>
        /// <param name="op">Operator</param>
        /// <param name="right">Right expression</param>
        public CompareExpr(Expr left, Operator op, Expr right)
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
        /// Operator > >= == != less less than
        /// </summary>
        public Operator Op;


        /// <summary>
        /// Right hand expression
        /// </summary>
        public Expr Right;


        /// <summary>
        /// Evaluate > >= != == less less than
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            bool result = false;
            object left = Left.Evaluate();
            object right = Right.Evaluate();

            if (left is int || left is long)
                left = Convert.ToDouble(left);
            if (right is int || right is long)
                right = Convert.ToDouble(right);

            // Both double
            if (left is double && right is double)
                result = CompareNumbers((double)left, (double)right, Op);

            // Both strings
            else if (left is string && right is string)
                result = CompareStrings((string)left, (string)right, Op);

            // Both bools
            else if (left is bool && right is bool)
                result = CompareNumbers(Convert.ToDouble(left), Convert.ToDouble(right), Op);

            // Both dates
            else if (left is DateTime && right is DateTime)
                result = CompareDates((DateTime)left, (DateTime)right, Op);

            // Both Timespans
            else if (left is TimeSpan && right is TimeSpan)
                result = CompareTimes((TimeSpan)left, (TimeSpan)right, Op);

            // 1 or both null
            else if (left == LNull.Instance || right == LNull.Instance)
                result = CompareNull(left, right, Op);

            // Day of week ?
            else if (left is DayOfWeek || right is DayOfWeek)
                result = CompareDays(left, right, Op);

            // Day of week ?
            else if (left is LUnit || right is LUnit)
                result = CompareUnits((LUnit)left, (LUnit)right, Op);

            else if (IsNumeric(left) && IsNumeric(right))
                result = CompareNumbers(Convert.ToDouble(left), Convert.ToDouble(right), Op);

            return result;
        }

        private bool IsNumeric(object value)
        {
            if (value is double || value is int || value is uint || value is long || value is ulong || value is short
                || value is ushort || value is byte || value is sbyte || value is float || value is decimal)
            {
                return true;
            }

            return false;
        }

        private static bool CompareNull(object left, object right, Operator op)
        {
            // Both null
            if (left == LNull.Instance && right == LNull.Instance && op == Operator.EqualEqual) return true;
            if (left == LNull.Instance && right == LNull.Instance && op == Operator.NotEqual) return false;
            // Both not null
            if (left != LNull.Instance && right != LNull.Instance && op == Operator.EqualEqual) return left == right;
            if (left != LNull.Instance && right != LNull.Instance && op == Operator.NotEqual) return left != right;
            // Check for one
            if (op == Operator.NotEqual) return true;

            return false;
        }


        private static bool CompareNumbers(double left, double right, Operator op)
        {
            if (op == Operator.LessThan) return left < right;
            if (op == Operator.LessThanEqual) return left <= right;
            if (op == Operator.MoreThan) return left > right;
            if (op == Operator.MoreThanEqual) return left >= right;
            if (op == Operator.EqualEqual) return left == right;
            if (op == Operator.NotEqual) return left != right;
            return false;
        }


        private static bool CompareStrings(string left, string right, Operator op)
        {
            if (op == Operator.EqualEqual) return left == right;
            if (op == Operator.NotEqual) return left != right;
            int compareResult = String.CompareOrdinal(left, right);
            if (op == Operator.LessThan) return compareResult == -1;
            if (op == Operator.LessThanEqual) return compareResult != 1;
            if (op == Operator.MoreThan) return compareResult == 1;
            if (op == Operator.MoreThanEqual) return compareResult != -1;

            return false;
        }


        private static bool CompareDates(DateTime left, DateTime right, Operator op)
        {
            if (op == Operator.LessThan) return left < right;
            if (op == Operator.LessThanEqual) return left <= right;
            if (op == Operator.MoreThan) return left > right;
            if (op == Operator.MoreThanEqual) return left >= right;
            if (op == Operator.EqualEqual) return left == right;
            if (op == Operator.NotEqual) return left != right;
            return false;
        }


        private static bool CompareTimes(TimeSpan left, TimeSpan right, Operator op)
        {
            if (op == Operator.LessThan) return left < right;
            if (op == Operator.LessThanEqual) return left <= right;
            if (op == Operator.MoreThan) return left > right;
            if (op == Operator.MoreThanEqual) return left >= right;
            if (op == Operator.EqualEqual) return left == right;
            if (op == Operator.NotEqual) return left != right;
            return false;
        }


        private static bool CompareUnits(LUnit left, LUnit right, Operator op)
        {
            if (op == Operator.LessThan) return left.BaseValue < right.BaseValue;
            if (op == Operator.LessThanEqual) return left.BaseValue <= right.BaseValue;
            if (op == Operator.MoreThan) return left.BaseValue > right.BaseValue;
            if (op == Operator.MoreThanEqual) return left.BaseValue >= right.BaseValue;
            if (op == Operator.EqualEqual) return left.BaseValue == right.BaseValue;
            if (op == Operator.NotEqual) return left.BaseValue != right.BaseValue;
            return false;
        }


        private static bool CompareDays(object left, object right, Operator op)
        {
            bool result = false;
            // Dates vs DayOfWeek
            if ((left is DateTime && right is DayOfWeek))
            {
                var leftDay = (int)((DateTime)left).DayOfWeek;
                var rightDay = (int)((DayOfWeek)right);
                result = CompareNumbers(leftDay, rightDay, op);
            }
            else if ((left is DayOfWeek && right is DateTime))
            {
                var leftDay = (int)((DayOfWeek)left);
                var rightDay = (int)((DateTime)right).DayOfWeek;
                result = CompareNumbers(leftDay, rightDay, op);
            }
            else if ((left is double && right is DayOfWeek))
            {
                var rightDay = (int)((DayOfWeek)right);
                result = CompareNumbers((double)left, rightDay, op);
            }
            else if ((left is DayOfWeek && right is double))
            {
                var leftDay = (int)((DayOfWeek)left);
                result = CompareNumbers(leftDay, (double)right, op);
            }
            else if (left is DayOfWeek && right is DayOfWeek)
            {
                var leftDay = (int)((DayOfWeek)left);
                var rightDay = (int)((DayOfWeek)right);
                result = CompareNumbers(leftDay, rightDay, op);
            }
            return result;
        }
    }
}
