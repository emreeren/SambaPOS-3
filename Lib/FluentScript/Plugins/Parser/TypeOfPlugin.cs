using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
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
            if (exp.IsNodeType(NodeTypes.SysNew) && _tokenIt.NextToken.Token == Tokens.RightParenthesis)
            {
                //typeExp.SupportsBoundary = true;
                //typeExp.BoundaryText = ")";
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
        public override object DoEvaluate(IAstVisitor visitor)
        {
            // 1. Check for function ( currently does not support functions as first class types )
            if (_exp.IsNodeType(NodeTypes.SysVariable) || _exp.IsNodeType(NodeTypes.SysFunctionCall))
            {
                var name = _exp.ToQualifiedName();
                var exists = _exp.SymScope.IsFunction(name);
                if (exists)
                    return new LString("function:" + name);
            }
            var obj = _exp.Evaluate(visitor);
            ExceptionHelper.NotNull(this, obj, "typeof");
            var lobj = (LObject) obj;
            var typename = lobj.Type.Name;

            if (lobj.Type == LTypes.Array || lobj.Type == LTypes.Map)
                typename = "object:" + typename;
            else if (lobj.Type == LTypes.Bool)
                typename = "boolean";
            else if (lobj.Type.TypeVal == TypeConstants.LClass)
                typename = "object:" + lobj.Type.FullName;

            return new LString(typename);
        }
    }
}
