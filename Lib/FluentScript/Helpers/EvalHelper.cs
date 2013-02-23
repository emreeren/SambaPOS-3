using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

using ComLib.Lang.Types;
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Parsing;


namespace ComLib.Lang.Helpers
{
    /// <summary>
    /// Helper class for datatypes.
    /// </summary>
    public class EvalHelper
    {
        public static Context Ctx;


        /// <summary>
        /// Evaluate * / + - % 
        /// </summary>
        /// <returns></returns>
        public static object EvalBinary(BinaryExpr expr)
        {
            // Validate
            object result = 0;
            var node = expr;
            var op = expr.Op;
            var left = (LObject)expr.Left.Evaluate();
            var right = (LObject)expr.Right.Evaluate();

            // Case 1: Both numbers
            if (IsTypeMatch(LTypes.Number, left, right))
            {
                result = EvalHelper.CalcNumbers(node, (LNumber)left, (LNumber)right, op);
            }
            // Case 2: Both times
            else if (IsTypeMatch(LTypes.Time, left, right))
            {
                result = EvalHelper.CalcTimes(node, (LTime)left, (LTime)right, op);
            }
            // Case 3: Both dates
            else if (IsTypeMatch(LTypes.Date, left, right))
            {
                result = EvalHelper.CalcDates(node, (LDate)left, (LDate)right, op);
            }
            // Case 4: Both strings.
            else if (IsTypeMatch(LTypes.String, left, right))
            {
                var strleft = ((LString)left).Value;
                var strright = ((LString)right).Value;

                // Check string limit.
                Ctx.Limits.CheckStringLength(node, strleft, strright);
                result = new LString(strleft + strright);
            }

            // MIXED TYPES
            // TODO: Needs to be improved with new code for types.
            // Case 5 : Double and Bool
            else if (left.Type == LTypes.Number && right.Type == LTypes.Bool)
            {
                var r = ((LBool)right).Value;
                var rval = r ? 1 : 0;
                result = EvalHelper.CalcNumbers(node, (LNumber)left, new LNumber(rval), op);
            }
            // Bool Double
            else if (left.Type == LTypes.Bool && right.Type == LTypes.Number)
            {
                var l = ((LBool)left).Value;
                var lval = l ? 1 : 0;
                result = EvalHelper.CalcNumbers(node, new LNumber(lval), (LNumber)right, op);
            }
            // Append as strings.
            else if (left.Type == LTypes.String && right.Type == LTypes.Bool)
            {
                var st1 = ((LString)left).Value + ((LBool)right).Value.ToString().ToLower();
                result = new LString(st1);
            }
            // Append as strings.
            else if (left.Type == LTypes.Bool && right.Type == LTypes.String)
            {
                var st2 = ((LBool)left).Value.ToString().ToLower() + ((LString)right).Value;
                result = new LString(st2);
            }
            // TODO: Need to handle LUnit and LVersion better
            //else if (left.Type == LTypes.Unit && right.Type == LTypes.Unit)
            else if (left.Type.Name == "LUnit" && right.Type.Name == "LUnit")
            {
                result = EvalHelper.CalcUnits(node, (LUnit)((LClass)left).Value, (LUnit)((LClass)right).Value, op, Ctx.Units);
            }
            else
            {
                var st3 = left.GetValue().ToString() + right.GetValue().ToString();
                result = new LString(st3);
            }
            return result;
        }


        /// <summary>
        /// Evaluate > >= != == less less than
        /// </summary>
        /// <returns></returns>
        public static object EvalCompare(CompareExpr expr)
        {
            // Validate
            object result = null;
            var node = expr;
            var op = expr.Op;
            var left = (LObject)expr.Left.Evaluate();
            var right = (LObject)expr.Right.Evaluate();


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

            // Units
            //else if (left.Type == LTypes.Unit || right.Type == LTypes.Unit)
            else if (left.Type.Name == "LUnit" || right.Type.Name == "LUnit")
                result = EvalHelper.CompareUnits(node, (LUnit)((LClass)left).Value, (LUnit)((LClass)right).Value, op);

            return result;
        }


        /// <summary>
        /// Evaluate > >= != == less less than
        /// </summary>
        /// <returns></returns>
        public static object EvalConditional(ConditionExpr expr)
        {
            // Validate
            var op = expr.Op;
            if (op != Operator.And && op != Operator.Or)
                throw new ArgumentException("Only && || supported");

            var result = false;
            var lhsVal = expr.Left.Evaluate();
            var rhsVal = expr.Right.Evaluate();
            var left = false;
            var right = false;
            if (lhsVal != null) left = ((LBool)lhsVal).Value;
            if (rhsVal != null) right = ((LBool)rhsVal).Value;

            if (op == Operator.Or)
            {
                result = left || right;
            }
            else if (op == Operator.And)
            {
                result = left && right;
            }
            return new LBool(result);
        }


        /// <summary>
        /// Evaluate a constant.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static object EvalConstant(object val)
        {
            // 1. Null
            if (val == LObjects.Null)
                return val;

            if (val is LObject)
                return val;

            // 2. Actual value.
            var ltype = LangTypeHelper.ConvertToLangValue(val);
            return ltype;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public static object EvalVariable(VariableExpr expr)
        {
            // Case 1: memory variable has highest precendence
            var name = expr.Name;
            if (Ctx.Memory.Contains(name))
            {
                var val = Ctx.Memory.Get<object>(name);
                return val;
            }
            // Case 2: check function now.
            if (expr.SymScope.IsFunction(name))
            {

            }
            throw ExceptionHelper.BuildRunTimeException(expr, "variable : " + name + " does not exist");
        }


        /// <summary>
        /// Evaluates an array type declaration.
        /// </summary>
        /// <returns></returns>
        public static object EvalArrayType(List<Expr> arrayExprs)
        {
            // Case 1: array type
            if (arrayExprs != null)
            {
                var items = new List<object>();

                foreach (var exp in arrayExprs)
                {
                    object result = exp == null ? null : exp.Evaluate();
                    items.Add(result);
                }
                var array = new LArray(items);
                return array;
            }
            return LObjects.Null;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public static object EvalMapType(List<Tuple<string, Expr>> mapExprs)
        {
            // Case 2: Map type
            var dictionary = new Dictionary<string, object>();
            foreach (var pair in mapExprs)
            {
                var expression = pair.Item2;
                object result = expression == null ? null : expression.Evaluate();
                dictionary[pair.Item1] = result;
            }
            var map = new LMap(dictionary);
            return map;
        }


        /// <summary>
        /// Creates new instance of the type.
        /// </summary>
        /// <returns></returns>
        public static object EvalNew(NewExpr expr)
        {
            object[] constructorArgs = null;
            var paramListExprs = expr.ParamListExpressions;
            if (paramListExprs != null && paramListExprs.Count > 0)
            {
                expr.ParamList = new List<object>();
                ParamHelper.ResolveNonNamedParameters(paramListExprs, expr.ParamList);
                constructorArgs = expr.ParamList.ToArray();
            }

            // CASE 1: Built in basic system types ( string, date, time, etc )
            if (LTypesLookup.IsBasicTypeShortName(expr.TypeName))
            {
                // TODO: Move this check to Semacts later
                var langType = LTypesLookup.GetLType(expr.TypeName);
                var methods = Ctx.Methods.Get(langType);
                var canCreate = methods.CanCreateFromArgs(constructorArgs);
                if (!canCreate)
                    throw ExceptionHelper.BuildRunTimeException(expr, "Can not create " + expr.TypeName + " from parameters");

                // Allow built in type methods to create it.
                var result = methods.CreateFromArgs(constructorArgs);
                return result;
            }
            // CASE 2: Custom types e.g. custom classes.
            var hostLangArgs = LangTypeHelper.ConvertToArrayOfHostLangValues(constructorArgs);
            var instance = Ctx.Types.Create(expr.TypeName, hostLangArgs);
            return new LClass(instance);
        }


        /// <summary>
        /// Evaluate object[index]
        /// </summary>
        /// <returns></returns>
        public static object EvalIndexAccess(IndexExpr expr)
        {
            var ndxVal = expr.IndexExp.Evaluate();
            var listObject = expr.VariableExp.Evaluate();

            // Check for empty objects.
            ExceptionHelper.NotNull(expr, listObject, "indexing");
            ExceptionHelper.NotNull(expr, ndxVal, "indexing");

            var lobj = (LObject)listObject;

            // CASE 1. Access 
            //      e.g. Array: users[0] 
            //      e.g. Map:   users['total']
            if (!expr.IsAssignment)
            {
                var result = EvalHelper.AccessIndex(Ctx.Methods, expr, lobj, (LObject)ndxVal);
                return result;
            }

            // CASE 2.  Assignment
            //      e.g. Array: users[0]        = 'john'
            //      e.g. Map:   users['total']  = 200
            // NOTE: In this case of assignment, return back a MemberAccess object descripting what is assign
            var indexAccess = new IndexAccess();
            indexAccess.Instance = lobj;
            indexAccess.MemberName = (LObject)ndxVal;
            return indexAccess;
        }


        /// <summary>
        /// Evaluates the expression by appending all the sub-expressions.
        /// </summary>
        /// <returns></returns>
        public static object EvalInterpolated(InterpolatedExpr expr)
        {
            if (expr.Expressions == null || expr.Expressions.Count == 0)
                return string.Empty;

            string total = "";
            foreach (var exp in expr.Expressions)
            {
                if (exp != null)
                {
                    var val = exp.Evaluate();
                    var text = "";
                    var lobj = (LObject)val;
                    text = lobj.GetValue().ToString();
                    total += text;
                }
            }
            return new LString(total);
        }


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
            var result = new LUnit();
            result.BaseValue = baseUnitsValue;
            result.Group = left.Group;
            result.SubGroup = left.SubGroup;
            result.Value = relativeValue;

            return new LClass(result);
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
            if (target.Type == LTypes.Array)
            {
                var ndx = ((LNumber)ndxObj).Value;
                var methods = regmethods.Get(LTypes.Array);

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
        /// Is match with the type supplied and the 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        private static bool IsTypeMatch(LType type, LObject obj1, LObject obj2)
        {
            if (obj1.Type == type && obj2.Type == type)
                return true;
            return false;
        }
    }
}
