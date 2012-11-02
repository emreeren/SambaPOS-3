using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
{

    /* *************************************************************************
    <doc:example>	
    // Type of plugins gets the type of an expression.
    
    function inc(a) { return a + 1; }
    
    dataType = typeof 'fluentscript'    // 'string'
    dataType = typeof 12                // 'number'
    dataType = typeof 12.34             // 'number'
    dataType = typeof true              // 'boolean'
    dataType = typeof false             // 'boolean'
    dataType = typeof new Date()        // 'datetime'
    dataType = typeof 3pm               // 'time'
    dataType = typeof [0, 1, 2]         // 'object:list'
    dataType = typeof { name: 'john' }  // 'object:map'   
    dataType = typeof new User('john')  // 'object:ComLib.Lang.Tests.Common.User'
    dataType = typeof inc               // 'function:inc'
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling swapping of variable values. swap a and b.
    /// </summary>
    public class TypeOfPlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public TypeOfPlugin()
        {
            this.IsStatement = false;
            this.IsAutoMatched = true;
            this.StartTokens = new string[] { "typeof" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "typeof <expression>";
            }
        }


        /// <summary>
        /// Examples
        /// </summary>
        public override string[] Examples
        {
            get
            {
                return new string[]
                {
                    "typeof 3",
                    "typeof 'abcd'",
                    "typeof user"
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {                        
            // The expression to round.
            _tokenIt.Advance(1, false);
            var exp = _parser.ParseExpression(null, false, true, true, false);
            var typeExp = new TypeOfExpr(exp);
            if (exp is NewExpr && _tokenIt.NextToken.Token == Tokens.RightParenthesis)
            {
                typeExp.SupportsBoundary = true;
                typeExp.BoundaryText = ")";
            }

            return typeExp;
        }
    }



    /// <summary>
    /// Variable expression data
    /// </summary>
    public class TypeOfExpr : Expr
    {
        private Expr _exp;
        
        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="exp">The expression value to round</param>
        public TypeOfExpr(Expr exp)
        {
            _exp = exp;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object Evaluate()
        {
            // 1. Check for function ( currently does not support functions as first class types )
            if (_exp is VariableExpr || _exp is FunctionCallExpr)
            {
                var name = _exp.ToQualifiedName();
                if (Ctx.Functions.Contains(name))
                    return "function:" + name;
            }
            var obj = _exp.Evaluate();
            object result = null;
            if (obj == null)
                return typeof(LNull);

            if (obj is string) result = "string";
            else if (obj is DateTime) result = "datetime";
            else if (obj is TimeSpan) result = "time";
            else if (obj is bool) result = "boolean";
            else if (obj is int || obj is long) result = "number";
            else if (obj is float || obj is double || obj is decimal) result = "number";
            else if (obj is LArray) result = "object:list";
            else if (obj is LMap) result = "object:map";
            else
            {
                var fullname = obj.GetType().FullName;
                result = "object:" + fullname;
            }

            return result;
        }
    }
}
