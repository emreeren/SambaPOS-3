using System;
using System.Collections.Generic;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Types;
// </lang:using>

namespace ComLib.Lang.Parsing
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
        /// <param name="sourceName"></param>
        /// <returns></returns>
        public static Expr ForEach(string varname, string sourceName, TokenData token)
        {
            var exp = new ForEachExpr();
            exp.VarName = varname;
            exp.SourceName = sourceName;
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
