using System;
using System.Collections.Generic;
using System.Collections;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Runtime.Bindings;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Runtime
{
    /// <summary>
    /// Helper class for datatypes.
    /// </summary>
    public class Execution : IAstVisitor
    {
        private Dictionary<int, int> _daysInMonth;

        /// <summary>
        /// Initialize
        /// </summary>
        public Execution()
        {
            _daysInMonth = new Dictionary<int, int>();
            _daysInMonth[1] = 31;
            _daysInMonth[2] = 28;
            _daysInMonth[3] = 31;
            _daysInMonth[4] = 30;
            _daysInMonth[5] = 31;
            _daysInMonth[6] = 30;
            _daysInMonth[7] = 31;
            _daysInMonth[8] = 31;
            _daysInMonth[9] = 30;
            _daysInMonth[10] = 31;
            _daysInMonth[11] = 30;
            _daysInMonth[12] = 31;
        }


        /// <summary>
        /// The execution context.
        /// </summary>
        public Context Ctx { get; set; }


        /// <summary>
        /// The execution settings.
        /// </summary>
        public ExecutionSettings Settings { get; set; }


        /// <summary>
        /// Visit all the expressions.
        /// </summary>
        /// <param name="exprs"></param>
        /// <returns></returns>
        public object VisitExprs(List<Expr> exprs)
        {
            object result = LObjects.Null;
            foreach (var expr in exprs)
            {
                result = expr.Evaluate(this);
            }
            return result;
        }


        /// <summary>
        /// Visit the statement
        /// </summary>
        /// <param name="exp"></param>
        public object VisitExpr(Expr exp)
        {
            return exp.Evaluate(this);
        }


        #region Statements
        /// <summary>
        /// Assign a value to an expression.
        /// </summary>
        /// <param name="expr">The assignment expressions</param>
        /// <returns></returns>
        public object VisitAssign(AssignExpr expr)
        {
            var ctx = expr.Ctx;
            var varExpr = expr.VarExp;
            var valueExpr = expr.ValueExp;
            var node = expr;
            var isDeclaration = expr.IsDeclaration;

            // CASE 1: Assign variable.  a = 1
            if (varExpr.IsNodeType(NodeTypes.SysVariable))
            {
                AssignHelper.SetVariableValue(ctx, this, node, isDeclaration, varExpr, valueExpr);
            }
            // CASE 2: Assign member.    
            //      e.g. dictionary       :  user.name = 'kishore'
            //      e.g. property on class:  user.age  = 20
            else if (varExpr.IsNodeType(NodeTypes.SysMemberAccess))
            {
                AssignHelper.SetMemberValue(ctx, this, node, varExpr, valueExpr);
            }
            // Case 3: Assign value to index: "users[0]" = <expression>;
            else if (varExpr.IsNodeType(NodeTypes.SysIndex))
            {
                AssignHelper.SetIndexValue(ctx, this, node, varExpr, valueExpr);
            }
            return LObjects.Null;
        }


        /// <summary>
        /// Executes multiple assignments.
        /// </summary>
        /// <returns></returns>
        public object VisitAssignMulti(AssignMultiExpr expr)
        {
            foreach (var assigment in expr.Assignments)
            {
                VisitAssign(assigment);
            }
            return LObjects.Null;
        }


        /// <summary>
        /// Execute the break.
        /// </summary>
        public object VisitBreak(BreakExpr expr)             
        {
            var loop = expr.FindParent<ILoop>();
            if (loop == null) 
                throw new LangException("syntax error", "unable to break, loop not found", string.Empty, 0);

            loop.DoBreakLoop = true;
            return LObjects.Null;
        }


        /// <summary>
        /// Execute the continue.
        /// </summary>
        public object VisitContinue(ContinueExpr expr)
        {
            var loop = expr.FindParent<ILoop>();
            if (loop == null) throw new LangException("syntax error", "unable to break, loop not found", string.Empty, 0);

            loop.DoContinueLoop = true;
            return LObjects.Null;
        }


        /// <summary>
        /// Execute the continue.
        /// </summary>
        public object VisitDate(DateExpr expr )
        {
            var year = expr.Year == -1 ? DateTime.Now.Year : expr.Year;
            var date = new DateTime(year, expr.Month, expr.Day);
            if(expr.Time != "0")
            {
                var t = TimeSpan.Parse(expr.Time);
                date = date.AddMilliseconds(t.TotalMilliseconds);
            }
            return new LDate(date);
        }


        public object VisitDateRelative(DateRelativeExpr expr)
        {
            var year = DateTime.Now.Year;
            var month = expr.Month;
            var dayofWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), expr.DayOfTheWeek.ToString());

            var date = new DateTime(year, month, 1);
            
            var DaysInWeek = 7;

            // move to holiday day of week
            var dayInc = 0;
            while (date.DayOfWeek != dayofWeek && dayInc <= DaysInWeek)
            {
                date = date.AddDays(1);
                dayInc++;
            }

            var relDay = expr.RelativeDay.ToLower();
                
            // Case 1: "last" <day> of <month>
            if( relDay == "last")
            {
                // 1. Add 2 weeks at least
                date = date.AddDays(14);

                var i = 0;
                var totalDaysInMonth = _daysInMonth[date.Month];
                while (i < 2)
                {
                    // 2. feb check
                    if (date.Month == 2 && (date.Day + 7) <= 28)
                    {
                        date = date.AddDays(7);
                    }
                    else if(date.Day + 7 <= totalDaysInMonth)
                    {
                        date = date.AddDays(7);
                    }
                    i++;
                }
            }
            // Case 2: 
            else
            {
                var weekCount = 0;
                if (relDay == "first") weekCount = 1;
                else if (relDay == "second") weekCount = 2;
                else if (relDay == "third") weekCount = 3;
                else if (relDay == "fourth") weekCount = 4;

                // Case 2a: "first|second|third|fourth" of month
                if (weekCount > 0)
                {
                    date = date.AddDays((weekCount - 1) * DaysInWeek);
                }
                // Case 2b: ( 1st | 2nd | 3rd| 4th ) of month 
                else
                {
                    weekCount = Convert.ToInt32(expr.RelativeDay);
                    date = date.AddDays((weekCount - 1) * DaysInWeek);
                }
            }            
            return new LDate(date);
        }


        /// <summary>
        /// Execute the continue.
        /// </summary>
        public object VisitDay(DayExpr expr)
        {
            var date = DateTime.Today;
            var dayOfweek = DayOfWeek.Monday;
            var isDayOfWeek = false;
            var isTimeSpecified = expr.Time != "0";

            var dayName = expr.Name.ToLower();

            // 1. Determine date/day
            if      (dayName == "today")     date = DateTime.Today;
            else if (dayName == "yesterday") date = DateTime.Today.AddDays(-1);
            else if (dayName == "tomorrow")  date = DateTime.Today.AddDays(1);
            else if (dayName == "monday")    { isDayOfWeek = true; dayOfweek = DayOfWeek.Monday;   }
            else if (dayName == "tuesday")   { isDayOfWeek = true; dayOfweek = DayOfWeek.Tuesday;   }
            else if (dayName == "wednesday") { isDayOfWeek = true; dayOfweek = DayOfWeek.Wednesday; }  
            else if (dayName == "thursday")  { isDayOfWeek = true; dayOfweek = DayOfWeek.Thursday;  }  
            else if (dayName == "friday")    { isDayOfWeek = true; dayOfweek = DayOfWeek.Friday;    } 
            else if (dayName == "saturday")  { isDayOfWeek = true; dayOfweek = DayOfWeek.Saturday;  }
            else if (dayName == "sunday")    { isDayOfWeek = true; dayOfweek = DayOfWeek.Sunday;    }

            // Case 1 -
            if (isDayOfWeek)
            {
                // Case 1a: day of week only
                if(!isTimeSpecified) 
                    return new LDayOfWeek(dayOfweek);

                // Case 1b: day of week ( with time )
                var today = DateTime.Today;
                var count = 0;
                while (today.DayOfWeek != dayOfweek && count < 8)
                {
                    today = today.AddDays(1);
                }                
            }
            
            // 3. Finally - add the time to the day.
            if(expr.Time != "0")
            {
                var t = TimeSpan.Parse(expr.Time);
                date = date.AddMilliseconds(t.TotalMilliseconds);
            }
            var result = new LDate(date);
            return result;
        }


        /// <summary>
        /// Executes the days away expresssion to get a date x days away.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object VisitDuration(DurationExpr expr)
        {
            var name = expr.Duration;
            var num = 0.0;

            // Case 1: daysAhead days away
            if(this.Ctx.Symbols.Contains(name))
            {
                var variable = this.Ctx.Memory.Get<object>(name) as LObject;
                if(variable.Type != LTypes.Number)
                {
                    throw new LangException("TypeError", "Days away must of type of number", expr.Ref.ScriptName, expr.Ref.Line, expr.Ref.CharPos);
                }
                num = ((LNumber) variable).Value;
            }

            // Case 2: 3 days away
            else
            {
                num = Convert.ToInt32(name);
            }
            
            // Now convert to relative "days away".
            TimeSpan duration = TimeSpan.MinValue;
            var mode = expr.Mode.ToLower();
            if (mode == "days")
            {
                duration = new TimeSpan(Convert.ToInt32(num), 0, 0, 0);
            }
            if (mode == "hours")
            {
                duration = new TimeSpan(0, Convert.ToInt32(num), 0, 0);
            } 
            if (mode == "minutes")
            {
                duration = new TimeSpan(0, 0, Convert.ToInt32(num), 0);
            } 
            if (mode == "seconds")
            {
                duration = new TimeSpan(0, 0, 0, Convert.ToInt32(num));
            }
            return new LTime(duration);
        }


        /// <summary>
        /// Execute the continue.
        /// </summary>
        public object VisitFunctionDeclare(FunctionDeclareExpr expr)
        {
            return LObjects.Null;
        }


        /// <summary>
        /// Execute
        /// </summary>
        public object VisitIf(IfExpr expr)
        {
            // Case 1: If is true
            var result = expr.Condition.Evaluate(this) as LObject;
            bool execIf = EvalHelper.IsTrue(result);
            object returnVal = LObjects.Null;
            if (execIf)
            {
                
                if (expr.Statements != null && expr.Statements.Count > 0)
                {
                    foreach (var stmt in expr.Statements)
                    {
                        returnVal = stmt.Evaluate(this);
                    }
                }
            }
            // Case 2: Else available to execute
            else if (expr.Else != null)
            {
                returnVal = expr.Else.Evaluate(this);
            }
            return returnVal;
        }


        /// <summary>
        /// Execute each expression.
        /// </summary>
        /// <returns></returns>
        public object VisitForEach(ForEachExpr expr)
        {
            expr.DoContinueRunning = true;
            expr.DoBreakLoop = false;
            expr.DoContinueLoop = false;

            // for(user in users)
            // Push scope for var name 
            var source = expr.SourceExpr.Evaluate(this) as LObject;
            
            // Check : 1. null object?
            if (source == LObjects.Null)
                return LObjects.Null;

            IEnumerator enumerator = null;
            if (source.Type == LTypes.Array) enumerator = ((IList)source.GetValue()).GetEnumerator();
            else if (source.Type == LTypes.Map) enumerator = ((IDictionary)source.GetValue()).GetEnumerator();
            else if (source.Type == LTypes.Table) 
                enumerator = ((IList) source.GetValue()).GetEnumerator();
            expr.DoContinueRunning = enumerator.MoveNext();

            while (expr.DoContinueRunning)
            {
                // Set the next value of "x" in for(x in y).
                var current = enumerator.Current is LObject ? enumerator.Current : LangTypeHelper.ConvertToLangClass(enumerator.Current);
                this.Ctx.Memory.SetValue(expr.VarName, current);

                if (expr.Statements != null && expr.Statements.Count > 0)
                {
                    foreach (var stmt in expr.Statements)
                    {
                        stmt.Evaluate(this);

                        this.Ctx.Limits.CheckLoop(expr);

                        // If Break statment executed.
                        if (expr.DoBreakLoop)
                        {
                            expr.DoContinueRunning = false;
                            break;
                        }
                        // Continue statement.
                        else if (expr.DoContinueLoop)
                            break;
                    }
                }
                else break;

                // Break loop here.
                if (expr.DoContinueRunning == false)
                    break;

                // Increment.
                expr.DoContinueRunning = enumerator.MoveNext();
            }
            return LObjects.Null;
        }


        public object VisitLambda(LambdaExpr expr)
        {
            var funcType = new LFunctionType();
            funcType.Name = expr.Expr.Meta.Name;
            funcType.FullName = funcType.Name;
            var func = new LFunction(expr.Expr);
            func.Type = funcType;
            return func;
        }


        /// <summary>
        /// Convert the presense / count of the enumerable expression(list/map) into bool true/false.
        /// </summary>
        /// <param name="expr">The list check expression.</param>
        /// <returns></returns>
        public object VisitListCheck(ListCheckExpr expr)
        {
            var varExp = expr.NameExp.Evaluate(this) as LObject;
            if (varExp == LObjects.Null)
                return new LBool(false);

            var count = 0;

            // 1. Array type ? check count
            if (varExp.Type == LTypes.Array)
            {
                count = ((LArray)varExp).Value.Count;
            }
            // 2 Table Map type 
            else if (varExp.Type == LTypes.Map)
            {
                count = ((LMap)varExp).Value.Count;
            }
            // 3. Table type
            else if (varExp.Type == LTypes.Table)
            {
                count = ((LTable) varExp).Value.Count;
            }
            // 3. Other type : keep count as 0 so we return false;
            var result = count > 0 ? new LBool(true) : new LBool(false);
            return result;
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public object VisitReturn(ReturnExpr expr)
        {
            var parent = expr.FindParent<FunctionExpr>();
            if (parent == null) throw new LangException("syntax error", "unable to return, parent not found", string.Empty, 0);

            object result = expr.Exp == null ? LObjects.Null : expr.Exp.Evaluate(this);
            bool hasReturnVal = expr.Exp != null;
            parent.HasReturnValue = hasReturnVal;
            parent.ReturnValue = result;
            parent.ContinueRunning = false;
            return LObjects.Null;
        }


        /// <summary>
        /// Execute
        /// </summary>
        public object VisitThrow(ThrowExpr expr)
        {
            var message = "";
            if (expr.Exp != null)
            {
                var result = expr.Exp.Evaluate(this) as LObject;
                if (result != LObjects.Null)
                    message = result.GetValue().ToString();
            }

            throw new LangException("TypeError", message, expr.Ref.ScriptName, expr.Ref.Line);
        }


        /// <summary>
        /// Execute
        /// </summary>
        public object VisitTryCatch(TryCatchExpr expr)
        {
            var tryScopePopped = false;
            var catchScopePopped = false;
            try
            {
                this.Ctx.Memory.Push();
                LangHelper.Evaluate(expr.Statements, expr, this);
                this.Ctx.Memory.Pop();
                tryScopePopped = true;
            }
            // Force the langlimit excpetion to propegate 
            // do not allow to flow through to the catch all "Exception ex".
            catch (LangLimitException)
            {
                throw;
            }
            catch (LangFailException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.Ctx.Limits.CheckExceptions(expr);

                // Pop the try scope.
                if (!tryScopePopped) this.Ctx.Memory.Pop();

                // Push the scope in the catch block
                this.Ctx.Memory.Push();
                var lException = LangTypeHelper.ConvertToLangClass(LError.FromException(ex));
                this.Ctx.Memory.SetValue(expr.ErrorName, lException);

                // Run statements in catch block.
                if (expr.Catch != null && expr.Catch.Statements.Count > 0)
                    LangHelper.Evaluate(expr.Catch.Statements, expr.Catch, this);

                // Pop the catch scope.
                this.Ctx.Memory.Pop();
                catchScopePopped = true;
            }
            finally
            {
                // Pop the catch scope in case there was an error.
                if (!catchScopePopped) this.Ctx.Memory.Remove(expr.ErrorName);
            }
            return LObjects.Null;
        }


        /// <summary>
        /// Execute
        /// </summary>
        public object VisitWhile(WhileExpr expr)
        {
            expr.DoContinueRunning = true;
            expr.DoBreakLoop = false;
            expr.DoContinueLoop = false;
            var result = expr.Condition.Evaluate(this) as LObject;
            expr.DoContinueRunning = EvalHelper.IsTrue(result);

            while (expr.DoContinueRunning)
            {
                if (expr.Statements != null && expr.Statements.Count > 0)
                {
                    foreach (var stmt in expr.Statements)
                    {
                        stmt.Evaluate(this);

                        this.Ctx.Limits.CheckLoop(expr);

                        // If Break statment executed.
                        if (expr.DoBreakLoop)
                        {
                            expr.DoContinueRunning = false;
                            break;
                        }
                        // Continue statement.
                        else if (expr.DoContinueLoop)
                            break;
                    }
                }
                else break;

                // Break loop here.
                if (expr.DoContinueRunning == false)
                    break;

                result = expr.Condition.Evaluate(this) as LObject;
                expr.DoContinueRunning = EvalHelper.IsTrue(result); 
            }
            return LObjects.Null;
        }


        /// <summary>
        /// Execute each expression.
        /// </summary>
        /// <returns></returns>
        public object VisitFor(ForExpr expr)
        {
            expr.Start.Evaluate(this);
            expr.DoContinueRunning = true;
            expr.DoBreakLoop = false;
            expr.DoContinueLoop = false;
            var result = expr.Condition.Evaluate(this) as LObject;
            expr.DoContinueRunning = EvalHelper.IsTrue(result);

            while (expr.DoContinueRunning)
            {
                if (expr.Statements != null && expr.Statements.Count > 0)
                {
                    foreach (var stmt in expr.Statements)
                    {
                        stmt.Evaluate(this);

                        this.Ctx.Limits.CheckLoop(expr);

                        // If Break statment executed.
                        if (expr.DoBreakLoop)
                        {
                            expr.DoContinueRunning = false;
                            break;
                        }
                        // Continue statement.
                        else if (expr.DoContinueLoop)
                            break;
                    }
                }
                else break;

                // Break loop here.
                if (expr.DoContinueRunning == false)
                    break;

                expr.Increment.Evaluate(this);
                result = expr.Condition.Evaluate(this) as LObject;
                expr.DoContinueRunning = EvalHelper.IsTrue(result); 
            }
            return LObjects.Null;
        }
        #endregion
        

        #region Expressions
        /// <summary>
        /// Visit the anyof expression.
        /// </summary>
        /// <param name="expr">AnyOf expression.</param>
        /// <returns></returns>
        public object VisitAnyOf(AnyOfExpr expr)
        {
            var result = false;
            var leftExpr = expr.CompareExpr;
            var leftResult = leftExpr.Evaluate(this) as LObject;
            if (expr.ParamListExpressions == null || expr.ParamListExpressions.Count == 0)
                return new LBool(result);

            // Resolve the parameters.
            ParamHelper.ResolveNonNamedParameters(expr.ParamListExpressions, expr.ParamList, this);

            foreach (var rvalue in expr.ParamList)
	        {
	            var rightResult = rvalue as LObject;
	            var compareResult = EvalHelper.Compare(expr, Operator.EqualEqual, leftResult, rightResult) as LObject;
                if (compareResult != null && compareResult.Type == LTypes.Bool && ((LBool)compareResult).Value == true)
		        {
			        result = true;
			        break;
		        }
	        }
	        return new LBool( result );
        }


        /// <summary>
        /// Evaluates an array type declaration.
        /// </summary>
        /// <returns></returns>
        public object VisitArray(ArrayExpr expr)
        {
            var arrayExprs = expr.Exprs;
            // Case 1: array type
            if (arrayExprs != null)
            {
                var items = new List<object>();

                foreach (var exp in arrayExprs)
                {
                    object result = exp == null ? null : exp.Evaluate(this);
                    items.Add(result);
                }
                var array = new LArray(items);
                return array;
            }
            return new LArray(new List<object>());
        }


        public object VisitRun(RunExpr expr)
        {
            // 1. visit the function call
            var result = expr.FuncCallExpr.Visit(this);
            
            // Case 1. Call something after?
            if (expr.FuncCallOnAfterExpr != null && expr.Mode == "after")
            {
                result = expr.FuncCallOnAfterExpr.Visit(this);
            }
            // Case 2. Call something with the input from the first ?
            else if (expr.FuncCallOnAfterExpr != null && expr.Mode == "on")
            {
                var funcCallExpr = expr.FuncCallOnAfterExpr as FunctionCallExpr;
                funcCallExpr.RetainEvaluatedParams = true;
                if(funcCallExpr.ParamList == null)
                    funcCallExpr.ParamList = new List<object>();
                funcCallExpr.ParamList.Add(result);
                funcCallExpr.Visit(this);
            }
            return result;
        }



        /// <summary>
        /// Evaluates a table declaration.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object VisitTable(TableExpr expr)
        {
            var table = new LTable(new List<object>());
            table.Fields = expr.Fields;
            return table;
        }
        
        
        /// <summary>
        /// Evaluate * / + - % 
        /// </summary>
        /// <returns></returns>
        public object VisitBinary(BinaryExpr expr)
        {
            // Validate
            object result = 0;
            var node = expr;
            var op = expr.Op;
            var left = (LObject) expr.Left.Evaluate(this);
            var right = (LObject) expr.Right.Evaluate(this);

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
                this.Ctx.Limits.CheckStringLength(node, strleft, strright);
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
            else if (left.Type.TypeVal == TypeConstants.Unit && right.Type.TypeVal == TypeConstants.Unit)
            {
                result = EvalHelper.CalcUnits(node, (LUnit)((LClass)left).Value, (LUnit)((LClass)right).Value, op, this.Ctx.Units);
            }
            else
            {
                // Check for null
                var lStringVal = "";
                var rStringVal = "";
                if (left != LObjects.Null)
                    lStringVal = left.GetValue().ToString();
                if (right != LObjects.Null)
                    rStringVal = right.GetValue().ToString();

                var st3 = lStringVal + rStringVal;
                result = new LString(st3);
            }
            return result;
        }


        /// <summary>
        /// Executes a call to a language binding class.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object VisitBindingCall(BindingCallExpr expr)
        {
            var method = expr.Name;

            // 1. Resolve the parameters.
            ParamHelper.ResolveParametersToHostLangValues(expr.ParamListExpressions, expr.ParamList, this);
            
            // 2. Push call into stack
            expr.Ctx.State.Stack.Push(expr.FullName, null);

            // 3. Call language binding method/function.
            var binding = new MetaCompiler();
            binding.Ctx = expr.Ctx;
            var result = binding.ExecuteFunction(method, new object[] { expr });

            result = FunctionHelper.CheckConvert(result);
            // 4. Pop the call stack
            expr.Ctx.State.Stack.Pop();
            return result;
        }


        /// <summary>
        /// Executes the block with callback/template methods.
        /// </summary>
        public object VisitBlock(BlockExpr expr)
        {
            object result = LObjects.Null;
            try
            {
                //expr.OnBlockEnter();
                expr.Ctx.Memory.Push();
                LangHelper.Evaluate(expr.Statements, expr.Parent, this);
            }
            finally
            {
                //expr.OnBlockExit();
                expr.Ctx.Memory.Pop();
            }
            return result;
        }


        /// <summary>
        /// Evaluate > >= != == less less than
        /// </summary>
        /// <returns></returns>
        public object VisitCompare(CompareExpr expr)
        {
            var node = expr;
            var op = expr.Op;

            // TODO: This should be here ( find a better solution )
            //  e.g. allow expression to support comparable ??
            if(expr.Right.Nodetype == NodeTypes.SysAnyOf)
            {
                var anyOf = ((AnyOfExpr) expr.Right);
                anyOf.CompareExpr = expr.Left;
                return this.VisitAnyOf(anyOf);
            }

            var left = (LObject) expr.Left.Evaluate(this);
            var right = (LObject) expr.Right.Evaluate(this);
            return EvalHelper.Compare(node, op, left, right);
        }


        /// <summary>
        /// Evaluate > >= != == less less than
        /// </summary>
        /// <returns></returns>
        public object VisitCondition(ConditionExpr expr)
        {
            // Validate
            var op = expr.Op;
            if (op != Operator.And && op != Operator.Or)
                throw new ArgumentException("Only && || supported");

            var result = false;
            var lhsVal = expr.Left.Evaluate(this);
            var rhsVal = expr.Right.Evaluate(this);
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
        public object VisitConstant(ConstantExpr expr)
        {
            var val = expr.Value;
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
        public object VisitVariable(VariableExpr expr)
        {
            // Case 1: memory variable has highest precendence
            var name = expr.Name;
            if (this.Ctx.Memory.Contains(name))
            {
                var val = this.Ctx.Memory.Get<object>(name);
                return val;
            }
            // Case 2: check function now.
            if (expr.SymScope.IsFunction(name))
            {

            }
            throw ExceptionHelper.BuildRunTimeException(expr, "variable : " + name + " does not exist");
        }


        // index here

        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public object VisitMap(MapExpr expr)
        {
            var mapExprs = expr.Expressions;
            // Case 2: Map type
            var dictionary = new Dictionary<string, object>();
            foreach (var pair in mapExprs)
            {
                var expression = pair.Item2;
                object result = expression == null ? null : expression.Evaluate(this);
                dictionary[pair.Item1] = result;
            }
            var map = new LMap(dictionary);
            return map;
        }


        /// <summary>
        /// Either external function or member name.
        /// </summary>
        /// <returns></returns>
        public object VisitMemberAccess(MemberAccessExpr expr)
        {
            var memberAccess = MemberHelper.GetMemberAccess(expr, this.Ctx, expr.VarExp, expr.MemberName, this);

            if (expr.IsAssignment)
                return memberAccess;

            if (memberAccess.MemberMissing)
                throw expr.BuildRunTimeException("Member : " + expr.MemberName + " does not exist");


            // NOTES:
            // 1. If property on a built in type && not assignment then just return the value of the property
            // 2. It's done here instead because there is no function/method call on a property.
            if (memberAccess.IsPropertyAccessOnBuiltInType())
            {
                var result = FunctionHelper.CallMemberOnBasicType(this.Ctx, expr, memberAccess, null, null, this);
                return result;
            }
            if (memberAccess.IsPropertyAccessOnClass() || memberAccess.IsFieldAccessOnClass())
            {
                var result = FunctionHelper.CallMemberOnClass(this.Ctx, expr, memberAccess, null, null, this);
                return result;
            }
            if (memberAccess.IsModuleAccess())
            {
                var result = MemberHelper.ResolveSymbol(memberAccess.Scope, expr.MemberName);
                return result;
            }
            return memberAccess;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public object VisitNamedParameter(NamedParameterExpr expr)
        {
            var result = expr.Value == null ? null : expr.Value.Evaluate(this);
            return result;
        }


        /// <summary>
        /// Creates new instance of the type.
        /// </summary>
        /// <returns></returns>
        public object VisitNew(NewExpr expr)
        {
            object[] constructorArgs = null;
            var paramListExprs = expr.ParamListExpressions;
            if (paramListExprs != null && paramListExprs.Count > 0)
            {
                expr.ParamList = new List<object>();
                ParamHelper.ResolveNonNamedParameters(paramListExprs, expr.ParamList, this);
                constructorArgs = expr.ParamList.ToArray();
            }

            // CASE 1: Built in basic system types ( string, date, time, etc )
            if (LTypesLookup.IsBasicTypeShortName(expr.TypeName))
            {
                // TODO: Move this check to Semacts later
                var langType = LTypesLookup.GetLType(expr.TypeName);
                var methods = this.Ctx.Methods.Get(langType);
                var canCreate = methods.CanCreateFromArgs(constructorArgs);
                if (!canCreate)
                    throw ExceptionHelper.BuildRunTimeException(expr, "Can not create " + expr.TypeName + " from parameters");

                // Allow built in type methods to create it.
                var result = methods.CreateFromArgs(constructorArgs);
                return result;
            }
            // CASE 2: Custom types e.g. custom classes.
            var hostLangArgs = LangTypeHelper.ConvertToArrayOfHostLangValues(constructorArgs);
            var instance = this.Ctx.Types.Create(expr.TypeName, hostLangArgs);
            var obj = LangTypeHelper.ConvertToLangClass(instance);
            return obj;
        }


        /// <summary>
        /// Evaluate object[index]
        /// </summary>
        /// <returns></returns>
        public object VisitIndex(IndexExpr expr)
        {
            var ndxVal = expr.IndexExp.Evaluate(this);
            var listObject = expr.VarExp.Evaluate(this);

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
        public object VisitInterpolated(InterpolatedExpr expr)
        {
            if (expr.Expressions == null || expr.Expressions.Count == 0)
                return string.Empty;

            string total = "";
            foreach (var exp in expr.Expressions)
            {
                if (exp != null)
                {
                    var val = exp.Evaluate(this);
                    var text = "";
                    var lobj = (LObject)val;
                    text = lobj.GetValue().ToString();
                    total += text;
                }
            }
            return new LString(total);
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public object VisitNegate(NegateExpr expr)
        {
            return EvalHelper.Negate(expr, this);
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public object VisitUnary(UnaryExpr expr)
        {
            // Logical not?
            if (expr.Op == Operator.LogicalNot)
                return EvalHelper.HandleLogicalNot(expr, this);

            var valobj = (LObject)expr.Ctx.Memory.Get<object>(expr.Name);

            // Double ? 
            if (valobj.Type == LTypes.Number)
                return EvalHelper.IncrementNumber(expr, (LNumber)valobj, this);

            // String ?
            if (valobj.Type == LTypes.String)
                return EvalHelper.IncrementString(expr, (LString)valobj, this);

            throw new LangException("Syntax Error", "Unexpected operation", expr.Ref.ScriptName, expr.Ref.Line, expr.Ref.CharPos);
        }


        /// <summary>
        /// Evauate and run the function
        /// </summary>
        /// <returns></returns>
        public object VisitFunctionCall(FunctionCallExpr expr)
        {
            object result = null;

            // CASE 1: Exp is variable -> internal/external script. "getuser()".            
            if (expr.NameExp.IsNodeType(NodeTypes.SysVariable))
            {
                return FunctionHelper.CallFunction(this.Ctx, expr, null, true, this);
            }

            // At this point, is a method call on an object.
            var member = expr.NameExp.Evaluate(this);
            result = member;
            var isMemberAccessType = member is MemberAccess;
            if (!isMemberAccessType) return result;

            var callStackName = expr.NameExp.ToQualifiedName();
            var maccess = member as MemberAccess;
            if (!IsMemberCall(maccess)) return result;

            this.Ctx.State.Stack.Push(callStackName, expr);
            // CASE 2: Module.Function
            if (maccess.Mode == MemberMode.FunctionScript && maccess.Expr != null)
            {
                var fexpr = maccess.Expr as FunctionExpr;
                var resolveParams = !expr.RetainEvaluatedParams;
                result = FunctionHelper.CallFunctionInScript(this.Ctx, this, fexpr.Meta.Name, fexpr, expr.ParamListExpressions, expr.ParamList, resolveParams);
            }
            // CASE 3: object "." method call from script is a external/internal function e.g log.error -> external c# callback.
            else if (maccess.IsInternalExternalFunctionCall())
            {
                result = FunctionHelper.CallFunction(Ctx, expr, maccess.FullMemberName, false, this);
            }
            // CASE 4: Method call / Property on Language types
            else if (maccess.Type != null)
            {
                result = FunctionHelper.CallMemberOnBasicType(this.Ctx, expr, maccess, expr.ParamListExpressions, expr.ParamList, this);
            }
            // CASE 5: Member call via "." : either static or instance method call. e.g. Person.Create() or instance1.FullName() e.g.
            else if (maccess.Mode == MemberMode.CustObjMethodStatic || maccess.Mode == MemberMode.CustObjMethodInstance)
            {
                result = FunctionHelper.CallMemberOnClass(this.Ctx, expr, maccess, expr.ParamListExpressions, expr.ParamList, this);
            }
            // Pop the function name off the call stack.
            this.Ctx.State.Stack.Pop();
            return result;
        }


        /// <summary>
        /// Visit the parameter expr.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object VisitParameter(ParameterExpr expr)
        {
            return LObjects.Null;
        }



        public object VisitFunction(FunctionExpr expr)
        {
            InitializeFunctionCall(expr);
            try
            {
                if (expr.Statements == null || expr.Statements.Count == 0)
                    return LObjects.Null;

                foreach (var statement in expr.Statements)
                {
                    statement.Evaluate(this);
                    if (!expr.ContinueRunning) break;
                }
            }
            catch (Exception ex)
            {
                expr.ErrorCount++;
                throw ex;
            }
            return LObjects.Null;
        }


        public void VisitBlockEnter(Expr expr)
        {
            this.Ctx.Memory.Push();
        }


        public void VisitBlockExit(Expr expr)
        {
            this.Ctx.Memory.Pop();
        }
        #endregion


        /// <summary>
        /// Is match with the type supplied and the 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        private bool IsTypeMatch(LType type, LObject obj1, LObject obj2)
        {
            if (obj1.Type == type && obj2.Type == type)
                return true;
            return false;
        }


        private bool IsMemberCall(MemberAccess maccess)
        {
            if (maccess.IsInternalExternalFunctionCall()
                || (maccess.Mode == MemberMode.MethodMember || maccess.Mode == MemberMode.PropertyMember && maccess.Type != null)
                || maccess.Mode == MemberMode.CustObjMethodInstance || maccess.Mode == MemberMode.CustObjMethodStatic
              )
                return true;
            return false;
        }

        private void InitializeFunctionCall(FunctionExpr expr)
        {
            // Keep track of total times this function was executed.
            // Keep tract of total times this function caused an error
            if (expr.ExecutionCount == long.MaxValue)
                expr.ExecutionCount = 0;
            else
                expr.ExecutionCount++;

            if (expr.ErrorCount == long.MaxValue)
                expr.ErrorCount = 0;

            expr.ContinueRunning = true;
            expr.ReturnValue = null;
            expr.HasReturnValue = false;

            PushParametersInScope(expr);
        }


        private void PushParametersInScope(FunctionExpr expr)
        {
            // 1. Validate : any arguments.
            if (expr.ArgumentValues == null || expr.ArgumentValues.Count == 0) return;
            if (expr.Meta.Arguments == null || expr.Meta.Arguments.Count == 0) return;

            // 2. Check if there is an parameter named "arguments"
            var hasParameterNamedArguments = false;
            if (expr.Meta.Arguments != null && expr.Meta.Arguments.Count > 0)
                if (expr.Meta.ArgumentsLookup.ContainsKey("arguments"))
                    hasParameterNamedArguments = true;

            // 3. Get the symbolscope of the inside of the function and see if any statements.
            ISymbols symscope = null;
            var hasStatements = expr.Statements != null && expr.Statements.Count > 0;
            if (hasStatements)
                symscope = expr.Statements[0].SymScope;

            // 3. Add function arguments to scope
            for (var ndx = 0; ndx < expr.Meta.Arguments.Count; ndx++)
            {
                var val = expr.ArgumentValues[ndx] as LObject;
                var arg = expr.Meta.Arguments[ndx];

                // 4. Clone primitive datatypes.
                if (val.Type.IsPrimitiveType())
                {
                    var copied = val.Clone();
                    expr.ArgumentValues[ndx] = copied;
                }

                // 5. Now, set the memory value of the parameter.
                this.Ctx.Memory.SetValue(arg.Name, val);

                // 6. Finally, update the symbol type
                if (hasStatements)
                {
                    var sym = symscope.GetSymbol(arg.Name);
                    if (sym != null && val.Type.TypeVal == TypeConstants.Function
                        && sym.Category != SymbolCategory.Func)
                    {
                        SymbolHelper.ResetSymbolAsFunction(symscope, arg.Name, val);
                    }
                }
            }

            // Finally add the arguments.
            // NOTE: Any extra arguments will be part of the implicit "arguments" array.
            if (!hasParameterNamedArguments)
            {
                var argArray = new LArray(expr.ArgumentValues);
                expr.Ctx.Memory.SetValue("arguments", argArray);
            }
        }
    }
}
