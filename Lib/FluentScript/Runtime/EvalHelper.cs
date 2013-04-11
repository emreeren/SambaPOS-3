using System;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Runtime
{
    /// <summary>
    /// Helper class for datatypes.
    /// </summary>
    public class EvalHelper
    {
        public static Context Ctx;


        /// <summary>
        /// Evalulates a math expression of 2 numbers.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The number on the left hand side</param>
        /// <param name="rhs">The number on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LNumber CalcNumbers(AstNode node, LNumber lhs, LNumber rhs, Operator op)
        {
            var left = lhs.Value;
            var right = rhs.Value;
            var result = 0.0;
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
            return new LNumber(result);
        }


        /// <summary>
        /// Evaluates a math expression of 2 units.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="left">The unit on the left</param>
        /// <param name="right">The unit on the right</param>
        /// <param name="op">The math operator</param>
        /// <param name="units"></param>
        /// <returns></returns>
        public static LObject CalcUnits(AstNode node, LUnit left, LUnit right, Operator op, Units units)
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
            var result = new LUnit(relativeValue);
            result.BaseValue = baseUnitsValue;
            result.Group = left.Group;
            result.SubGroup = left.SubGroup;
            //result.Value = relativeValue;
            var lclass = LangTypeHelper.ConvertToLangUnit(result);
            return lclass;
        }


        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The time on the left hand side</param>
        /// <param name="rhs">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LTime CalcTimes(AstNode node, LTime lhs, LTime rhs, Operator op)
        {
            if (op != Operator.Add && op != Operator.Subtract)
                throw ExceptionHelper.BuildRunTimeException(node, "Can only add/subtract times");

            var left = lhs.Value;
            var right = rhs.Value;
            var result = TimeSpan.MinValue;
            if (op == Operator.Add)
            {
                result = left + right;
            }
            else if (op == Operator.Subtract)
            {
                result = left - right;
            }
            return new LTime(result);
        }


        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The time on the left hand side</param>
        /// <param name="rhs">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LTime CalcDates(AstNode node, LDate lhs, LDate rhs, Operator op)
        {
            if (op != Operator.Subtract)
                throw ExceptionHelper.BuildRunTimeException(node, "Can only subtract dates");

            var left = lhs.Value;
            var right = rhs.Value;
            var result = left - right;
            return new LTime(result);
        }


        /// <summary>
        /// Increments the number supplied.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="op"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static LNumber CalcUnary(LNumber num, Operator op, double increment)
        {
            var val = num.Value;
            if (op == Operator.PlusPlus)
            {
                val++;
            }
            else if (op == Operator.MinusMinus)
            {
                val--;
            }
            else if (op == Operator.PlusEqual)
            {
                val = val + increment;
            }
            else if (op == Operator.MinusEqual)
            {
                val = val - increment;
            }
            else if (op == Operator.MultEqual)
            {
                val = val * increment;
            }
            else if (op == Operator.DivEqual)
            {
                val = val / increment;
            }
            return new LNumber(val);
        }


        /// <summary>
        /// Compares null values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public static LBool CompareNull(object left, object right, Operator op)
        {
            var result = false;
            if (left == LObjects.Null && right == LObjects.Null) 
                result = op == Operator.EqualEqual;
            else 
                result = op == Operator.NotEqual;

            return new LBool(result);
        }



        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The time on the left hand side</param>
        /// <param name="rhs">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LBool CompareNumbers(AstNode node, LNumber lhs, LNumber rhs, Operator op)
        {
            var left = lhs.Value;
            var right = rhs.Value;
            var result = false;
            if (op == Operator.LessThan)            result = left < right;
            else if (op == Operator.LessThanEqual)  result = left <= right;
            else if (op == Operator.MoreThan)       result = left > right;
            else if (op == Operator.MoreThanEqual)  result = left >= right;
            else if (op == Operator.EqualEqual)     result = left == right;
            else if (op == Operator.NotEqual)       result = left != right;
            return new LBool(result);
        }


        /// <summary>
        /// Visita a compare expression with the values evaluated.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="op"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static object Compare(AstNode node, Operator op, LObject left, LObject right)
        {
            object result = null;

            // Both double
            if (left.Type == LTypes.Number && right.Type == LTypes.Number)
                result = EvalHelper.CompareNumbers(node, (LNumber)left, (LNumber)right, op);

            // Both strings
            else if (left.Type == LTypes.String && right.Type == LTypes.String)
                result = EvalHelper.CompareStrings(node, (LString)left, (LString)right, op);

            // Both bools
            else if (left.Type == LTypes.Bool && right.Type == LTypes.Bool)
                result = EvalHelper.CompareBools(node, (LBool)left, (LBool)right, op);

            // Both dates
            else if (left.Type == LTypes.Date && right.Type == LTypes.Date)
                result = EvalHelper.CompareDates(node, (LDate)left, (LDate)right, op);

            // Both Timespans
            else if (left.Type == LTypes.Time && right.Type == LTypes.Time)
                result = EvalHelper.CompareTimes(node, (LTime)left, (LTime)right, op);

            // 1 or both null
            else if (left == LObjects.Null || right == LObjects.Null)
                result = EvalHelper.CompareNull(left, right, op);

            // Day of week ?
            else if (left.Type == LTypes.DayOfWeek || right.Type == LTypes.DayOfWeek)
                result = EvalHelper.CompareDays(node, left, right, op);

            // Date and time ?
            else if ((left.Type == LTypes.Date && right.Type == LTypes.Time)
                    || (left.Type == LTypes.Time && right.Type == LTypes.Date))
                result = EvalHelper.CompareDayDifference(node, left, right, op);

            // Units
            //else if (left.Type == LTypes.Unit || right.Type == LTypes.Unit)
            else if (left.Type.Name == "LUnit" || right.Type.Name == "LUnit")
                result = EvalHelper.CompareUnits(node, (LUnit)((LClass)left).Value, (LUnit)((LClass)right).Value, op);

            return result;
        }


        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The time on the left hand side</param>
        /// <param name="rhs">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LBool CompareBools(AstNode node, LBool lhs, LBool rhs, Operator op)
        {
            var left = Convert.ToInt32(lhs.Value);
            var right = Convert.ToInt32(rhs.Value);
            var result = false;
            if (op == Operator.LessThan)        result = left < right;
            else if (op == Operator.LessThanEqual)   result = left <= right;
            else if (op == Operator.MoreThan)        result = left > right;
            else if (op == Operator.MoreThanEqual)   result = left >= right;
            else if (op == Operator.EqualEqual)      result = left == right;
            else if (op == Operator.NotEqual)        result = left != right;
            return new LBool(result);
        }


        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The time on the left hand side</param>
        /// <param name="rhs">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LBool CompareStrings(AstNode node, LString lhs, LString rhs, Operator op)
        {
            var left = lhs.Value;
            var right = rhs.Value;
            var result = false;
            if (op == Operator.EqualEqual)
            {
                result = left == right;
                return new LBool(result);
            }
            else if (op == Operator.NotEqual)
            {
                result = left != right;
                return new LBool(result);
            }

            int compareResult = string.Compare(left, right, StringComparison.InvariantCultureIgnoreCase);
            if (op == Operator.LessThan) result = compareResult == -1;
            else if (op == Operator.LessThanEqual) result = compareResult != 1;
            else if (op == Operator.MoreThan) result = compareResult == 1;
            else if (op == Operator.MoreThanEqual) result = compareResult != -1;
            return new LBool(result);
        }


        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The time on the left hand side</param>
        /// <param name="rhs">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LBool CompareDates(AstNode node, LDate lhs, LDate rhs, Operator op)
        {
            var left = lhs.Value;
            var right = rhs.Value;
            var result = false;
            if (op == Operator.LessThan)             result = left < right;
            else if (op == Operator.LessThanEqual)   result = left <= right;
            else if (op == Operator.MoreThan)        result = left > right;
            else if (op == Operator.MoreThanEqual)   result = left >= right;
            else if (op == Operator.EqualEqual)      result = left == right;
            else if (op == Operator.NotEqual)        result = left != right;
            return new LBool(result);
        }


        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhSide">The time on the left hand side</param>
        /// <param name="rhSide">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LBool CompareDayDifference(AstNode node, LObject lhSide, LObject rhSide, Operator op)
        {
            var today = DateTime.Today;
            var targetDate = DateTime.Today;
            TimeSpan expectedDiff = TimeSpan.MinValue;
            if(lhSide.Type == LTypes.Date)
            {
                targetDate = ((LDate)lhSide).Value;
                expectedDiff = ((LTime)rhSide).Value;
            }
            else
            {
                targetDate = ((LDate)rhSide).Value;
                expectedDiff = ((LTime)lhSide).Value;
            }
            // Normalized to dates.
            var diff = targetDate - today;

            // Now compare if days away.
            var diffDays = diff.Days;
            if (diffDays < 0)
                return new LBool(false);

            //if (diffDays < 0) diffDays = diffDays*-1;

            // var diffHours = diff.Hours*-1;

            var result = false;
            if (op == Operator.LessThan)            result = diffDays <  expectedDiff.Days;
            else if (op == Operator.LessThanEqual)  result = diffDays <= expectedDiff.Days;
            else if (op == Operator.MoreThan)       result = diffDays >  expectedDiff.Days;
            else if (op == Operator.MoreThanEqual)  result = diffDays >= expectedDiff.Days;
            else if (op == Operator.EqualEqual)     result = diffDays == expectedDiff.Days;
            else if (op == Operator.NotEqual)       result = diffDays != expectedDiff.Days;
            return new LBool(result);
        }


        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The time on the left hand side</param>
        /// <param name="rhs">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LBool CompareTimes(AstNode node, LTime lhs, LTime rhs, Operator op)
        {
            var left = lhs.Value;
            var right = rhs.Value;
            var result = false;
            if (op == Operator.LessThan)             result = left < right;
            else if (op == Operator.LessThanEqual)   result = left <= right;
            else if (op == Operator.MoreThan)        result = left > right;
            else if (op == Operator.MoreThanEqual)   result = left >= right;
            else if (op == Operator.EqualEqual)      result = left == right;
            else if (op == Operator.NotEqual)        result = left != right;
            return new LBool(result);
        }


        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The time on the left hand side</param>
        /// <param name="rhs">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LBool CompareUnits(AstNode node, LUnit lhs, LUnit rhs, Operator op)
        {
            var result = false;
            if (op == Operator.LessThan)             result = lhs.BaseValue < rhs.BaseValue;
            else if (op == Operator.LessThanEqual)   result = lhs.BaseValue <= rhs.BaseValue;
            else if (op == Operator.MoreThan)        result = lhs.BaseValue > rhs.BaseValue;
            else if (op == Operator.MoreThanEqual)   result = lhs.BaseValue >= rhs.BaseValue;
            else if (op == Operator.EqualEqual)      result = lhs.BaseValue == rhs.BaseValue;
            else if (op == Operator.NotEqual)        result = lhs.BaseValue != rhs.BaseValue;
            return new LBool(result);
        }


        
        /// <summary>
        /// Evaluates a math expression of 2 time spans.
        /// </summary>
        /// <param name="node">The AST node the evaluation is a part of.</param>
        /// <param name="lhs">The time on the left hand side</param>
        /// <param name="rhs">The time on the right hand side</param>
        /// <param name="op">The math operator.</param>
        /// <returns></returns>
        public static LBool CompareDays(AstNode node, LObject lhs, LObject rhs, Operator op)
        {
            var left = LangTypeHelper.ConverToLangDayOfWeekNumber(lhs);
            var right = LangTypeHelper.ConverToLangDayOfWeekNumber(rhs);
            var res = CompareNumbers(node, left, right, op);
            return res;
        }


        public static LString IncrementString(UnaryExpr expr, LString sourceVal, IAstVisitor visitor)
        {
            // Check 1: Can only do += on strings.
            if (expr.Op != Operator.PlusEqual)
                throw new LangException("Syntax Error", "string operation with " + expr.Op.ToString() + " not supported", expr.Ref.ScriptName, expr.Ref.Line, expr.Ref.CharPos);

            //expr.DataType = typeof(string);
            var val = expr.Expression.Evaluate(visitor) as LObject;

            // Check 2: Check for null
            if (val == LObjects.Null)
                return sourceVal;

            // Check 3: Limit size if string
            Ctx.Limits.CheckStringLength(expr, sourceVal.Value, val.GetValue().ToString());

            // Finally do the appending.
            var appended = sourceVal.Value + val.GetValue().ToString();
            sourceVal.Value = appended;
            expr.Ctx.Memory.SetValue(expr.Name, sourceVal);
            return sourceVal;
        }


        public static LNumber IncrementNumber(UnaryExpr expr, LNumber val, IAstVisitor visitor)
        {
            var inc = expr.Increment == 0 ? 1 : expr.Increment;
            if (expr.Expression != null)
            {
                var incval = expr.Expression.Evaluate(visitor);
                // TODO: Check if null and throw langexception?
                inc = ((LNumber)incval).Value;
            }

            // 1. Calculate the unary value
            val = EvalHelper.CalcUnary(val, expr.Op, inc);

            // 2. Set the value back into scope
            expr.Ctx.Memory.SetValue(expr.Name, val);
            return val;
        }


        public static object HandleLogicalNot(UnaryExpr expr, IAstVisitor visitor)
        {
            var result = expr.Expression.Evaluate(visitor) as LObject;

            // Check 1:  This is actually an assert and should not happen.
            if (result == null)
                throw ExceptionHelper.BuildRunTimeException(expr, "Null value encountered");

            var retVal = false;

            // Only handle bool for logical not !true !false
            if (result.Type == LTypes.Bool)
                retVal = !((LBool)result).Value;
            else if (result == LObjects.Null)
                retVal = true;

            return new LBool(retVal);
        }


        public static object Negate(NegateExpr expr, IAstVisitor visitor)
        {
            var result = expr.Expression.Visit(visitor) as LObject;
            if (result == null)
                throw ExceptionHelper.BuildRunTimeException(expr, "Null value encountered");
            
            // Negate number.
            if(result.Type == LTypes.Number)
            {
                var retVal = ((LNumber) result).Value;
                retVal = retVal*-1;
                return new LNumber(retVal);
            }
            throw ExceptionHelper.BuildRunTimeException(expr, "Can only convert a number to a negative value");
        }


        /// <summary>
        /// Evaluate the result of indexing an object e.g. users[0] or users["admins"]
        /// </summary>
        /// <param name="regmethods"></param>
        /// <param name="node"></param>
        /// <param name="target"></param>
        /// <param name="ndxObj"></param>
        /// <returns></returns>
        public static LObject AccessIndex(RegisteredMethods regmethods, AstNode node, LObject target, LObject ndxObj)
        {
            object result = LObjects.Null;
            // Case 1: Array access users[0];
            if (target.Type == LTypes.Array || target.Type.TypeVal == LTypes.Table.TypeVal)
            {
                var ndx = ((LNumber)ndxObj).Value;
                var isArray = target.Type == LTypes.Array;
                var methods = isArray ? regmethods.Get(LTypes.Array) : regmethods.Get(LTypes.Table);

                // TODO: Make this generic.
                var length = Convert.ToInt32(methods.ExecuteMethod(target, "length", null));
                if(ndx >= length)
                    throw ExceptionHelper.BuildRunTimeException(node, "Index out of bounds : '" + ndx + "'");

                result = methods.GetByNumericIndex(target, (int)ndx);
            }
            // Case 2: Map access. users["kishore"];
            else if (target.Type == LTypes.Map)
            {
                var memberName = ((LString)ndxObj).Value;
                var methods = regmethods.Get(LTypes.Map);
                if (!methods.HasProperty(target, memberName))
                    throw ExceptionHelper.BuildRunTimeException(node, "Property does not exist : '" + memberName + "'");

                result = methods.GetByStringMember(target, memberName);
            }
            // Conver to lang type.
            if(result != LObjects.Null && !(result is LObject))
            {
                result = LangTypeHelper.ConvertToLangValue(result);
            }
            return (LObject)result;
        }


        /// <summary>
        /// Check if the expression is true.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IsTrue(LObject result)
        {
            if (result == null || result == LObjects.Null) return false;
            if (result.Type == LTypes.Number)
            {
                var num = (LNumber) result;
                return num.Value > 0;
            }
            if (result.Type == LTypes.String)
            {
                var str = (LString)result;
                return str.Value != null;
            }
            if (result.Type == LTypes.Bool)
            {
                var bl = (LBool) result;
                return bl.Value;
            }
            if ( result.Type == LTypes.Date)
            {
                var dt = (LDate) result;
                return dt.Value != DateTime.MinValue && dt.Value != DateTime.MaxValue;
            }
            return true;
        }
    }
}
