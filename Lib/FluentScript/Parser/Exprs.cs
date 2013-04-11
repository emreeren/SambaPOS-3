using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser
{
    /// <summary>
    /// Uses the Lexer to parse script in terms of sequences of Statements and Expressions;
    /// Each statement and expression is a sequence of Tokens( see Lexer )
    /// Main method is Parse(script) and ParseStatement();
    /// 
    /// 1. var name = "kishore";
    /// 2. if ( name == "kishore" ) print("true");
    /// 
    /// Statements:
    /// 
    /// VALUE:         TYPE:
    /// 1. AssignStmt ( "var name = "kishore"; )
    /// 2. IfStmt ( "if (name == "kishore" ) { print ("true"); }
    /// </summary>
    public class Exprs
    {
        private static TokenIterator _tokenIt;
        private static Context _ctx;
        private static string _scriptName;
        private static Stack<string> _withStack;


        /// <summary>
        /// Sets up the reference to token iterator and context
        /// </summary>
        /// <param name="tk"></param>
        /// <param name="ctx"></param>
        public static void Setup(TokenIterator tk, Context ctx, string scriptName)
        {
            _tokenIt = tk;
            _ctx = ctx;
            _scriptName = scriptName;
            _withStack = new Stack<string>();
        }


        /// <summary>
        /// Pushes the name on the with stack to use in "with" expressions.
        /// </summary>
        /// <param name="name"></param>
        public static void WithPush(string name)
        {
            _withStack.Push(name);           
        }


        /// <summary>
        /// Pops the last variable on the with stack for use in "with" expressions.
        /// </summary>
        public static void WithPop()
        {
            if (_withStack.Count > 0)
                _withStack.Pop();
        }


        /// <summary>
        /// The total number of items on the with stack.
        /// </summary>
        public static int WithCount()
        {
            return _withStack.Count;
        }


        /// <summary>
        /// Gets the name on the with stack.
        /// </summary>
        /// <returns></returns>
        public static string WithName()
        {
            return _withStack.Peek();
        }


        /// <summary>
        /// Creates a variable expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Expr Ident(string name, TokenData token)
        {            
            var exp = new VariableExpr();
            exp.Name = name;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a variable expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Expr IdentWith(TokenData token)
        {
            var exp = new VariableExpr();
            exp.Name = _withStack.Peek();
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates an index expression from the parameters supplied.
        /// </summary>
        /// <param name="varExp"></param>
        /// <param name="indexExp"></param>
        /// <param name="isAssignment"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Index(Expr varExp, Expr indexExp, bool isAssignment, TokenData token)
        {
            var exp = new IndexExpr();
            exp.IsAssignment = isAssignment;
            exp.VarExp = varExp;
            exp.IndexExp = indexExp;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a variable expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Expr Const(LObject obj, TokenData token)
        {
            var exp = new ConstantExpr();
            exp.Value = obj;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a date expression with symbol scope, context, script refernce set.
        /// The date expression can handle relative dates: 'today', 'yesterday', 'tomorrow'
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Expr Day(string name, string time, TokenData token)
        {
            var exp = new DayExpr();
            exp.Name = name;
            exp.Time = time;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a date expression.
        /// </summary>
        /// <param name="month">The month from 1 - 12</param>
        /// <param name="day">The day. required</param>
        /// <param name="year">The year ( can be -1 ) to get current year</param>
        /// <param name="time">The time in minutes as string e.g. "450" minutes = 7:30 am.</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Date(int month, int day, int year, string time, TokenData token)
        {
            var exp = new DateExpr();
            exp.Month = month;
            exp.Day = day;
            exp.Year = year;
            exp.Time = time;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a relative date expression with symbol scope, context, script refernce set.
        /// The date expression can handle relative dates: 3rd monday of january
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr DateRelative(string relativeDay, int dayOfWeek, int month, TokenData token)
        {
            var exp = new DateRelativeExpr();
            exp.RelativeDay = relativeDay;
            exp.Month = month;
            exp.DayOfTheWeek = dayOfWeek;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a relative date expression with symbol scope, context, script refernce set.
        /// The date expression can handle relative dates: 3rd monday of january
        /// </summary>
        /// <param name="daysAway">The number of days away.</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Duration(string duration, string mode, TokenData token)
        {
            var exp = new DurationExpr();
            exp.Duration = duration;
            exp.Mode = mode;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates an array expression from the parameters supplied.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Array(List<Expr> items, TokenData token)
        {
            var exp = new ArrayExpr();
            exp.Exprs = items;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates an array expression from the parameters supplied.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Run(string funcName, Expr funcCallExpr, TokenData token)
        {
            var exp = new RunExpr();
            exp.FuncName = funcName;
            exp.FuncCallExpr = funcCallExpr;
            SetupContext(exp, token);
            return exp;
        }
        
        
        /// <summary>
        /// Creates an array expression from the parameters supplied.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Table(List<string> fields, TokenData token)
        {
            var exp = new TableExpr();
            exp.Fields = fields;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a map expression from the parameters supplied.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Map(List<Tuple<string, Expr>> items, TokenData token)
        {
            var exp = new MapExpr();
            exp.Expressions = items;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a unary expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Unary(string name, Expr incExpr, double incValue, Operator op, TokenData token)
        {
            var exp = new UnaryExpr();
            exp.Name = name;
            exp.Op = op;
            exp.Increment = incValue;
            exp.Expression = incExpr;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a unary expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Negate(Expr expr, TokenData token)
        {
            var exp = new NegateExpr();
            exp.Expression = expr;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a unary expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Assign(bool declare, Expr left, Expr right, TokenData token)
        {
            var exp = new AssignExpr();
            exp.IsDeclaration = declare;
            exp.VarExp = left;
            exp.ValueExp = right;
            SetupContext(exp, token);
            return exp;
        }


        public static Expr AssignMulti(bool declare, List<AssignExpr> exprs, TokenData token)
        {
            var exp = new AssignMultiExpr();
            exp.Assignments = exprs;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a unary expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr For(Expr start, Expr condition, Expr increment, TokenData token)
        {
            var exp = new ForExpr();
            exp.Start = start;
            exp.Condition = condition;
            exp.Increment = increment;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a unary expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="varname"></param>
        /// <param name="sourceExpr"></param>
        /// <returns></returns>
        public static Expr ForEach(string varname, Expr sourceExpr, TokenData token)
        {
            var exp = new ForEachExpr();
            exp.VarName = varname;
            exp.SourceExpr = sourceExpr;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a unary expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Compare(Expr left, Operator op, Expr right, TokenData token)
        {
            var exp = new CompareExpr();
            exp.Left = left;
            exp.Op = op;
            exp.Right = right;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a unary expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Binary(Expr left, Operator op, Expr right, TokenData token)
        {
            var exp = new BinaryExpr();
            exp.Left = left;
            exp.Op = op;
            exp.Right = right;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a unary expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr Condition(Expr left, Operator op, Expr right, TokenData token)
        {
            var exp = new ConditionExpr();
            exp.Left = left;
            exp.Op = op;
            exp.Right = right;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates an expr that checks if the list variable supplied has any items.
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr ListCheck(TokenData name, TokenData token)
        {
            var exp = new ListCheckExpr();
            exp.NameExp = Ident(name.Token.Text, name);
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a unary expression with symbol scope, context, script refernce set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr NamedParam(string paramName, Expr val, TokenData token)
        {
            var exp = new NamedParameterExpr();
            exp.Name = paramName;
            exp.Value = val;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Creates a function call expression.
        /// </summary>
        /// <param name="nameExpr"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr FunctionCall(Expr nameExpr, List<Expr> parameters, TokenData token)
        {
            var funcExp = new FunctionCallExpr();
            funcExp.NameExp = nameExpr;
            funcExp.ParamListExpressions = parameters == null ? new List<Expr>() : parameters;
            funcExp.ParamList = new List<object>();
            SetupContext(funcExp, token);
            return funcExp;
        }


        public static Expr BindingCall(string bindingName, string functionName, TokenData token)
        {
            var bexpr = new BindingCallExpr();
            bexpr.Name = functionName;
            bexpr.FullName = "sys." + bindingName + "." + functionName;
            bexpr.ParamListExpressions = new List<Expr>();
            bexpr.ParamList = new List<object>();
            return bexpr;
        }


        /// <summary>
        /// Creates a function call expression.
        /// </summary>
        /// <param name="nameExpr"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Expr MemberAccess(Expr nameExpr, string memberName, bool isAssignment, TokenData token)
        {
            var exp = new MemberAccessExpr();
            exp.IsAssignment = isAssignment;
            exp.VarExp = nameExpr;
            exp.MemberName = memberName;
            SetupContext(exp, token);
            return exp;
        }


        /// <summary>
        /// Sets up the context, symbol scope and script source reference for the expression supplied.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="token"></param>
        public static void SetupContext(Expr expr, TokenData token)
        {
            if (expr == null) return;

            var reftoken = (token == null && _tokenIt != null) ? _tokenIt.NextToken : token;
            expr.Ctx = _ctx;
            if(expr.SymScope == null) expr.SymScope = _ctx.Symbols.Current;
            if(expr.Token == null )   expr.Token = reftoken;
            if(expr.Ref == null  && token != null )  
                expr.Ref = new ScriptRef(_scriptName, reftoken.Line, reftoken.LineCharPos);
        }
    }
}
