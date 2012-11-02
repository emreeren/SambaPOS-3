using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang
{

    /* *************************************************************************
    <doc:example>	
    // Return plugin provides return values
    
    return false;
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin for throwing errors from the script.
    /// </summary>
    public class ReturnPlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public ReturnPlugin()
        {
            this.ConfigureAsSystemExpression(false, true, "return");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "return <expression> <statementterminator>"; }
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
                    "return 3;",
                    "return 3\r\n",
                    "return result",
                    "return add(1,2)"
                };
            }
        }


        /// <summary>
        /// return value;
        /// </summary>
        /// <returns></returns>
        public override Expr  Parse()
        {
            var stmt = new ReturnExpr();
            _tokenIt.Expect(Tokens.Return);
            if (_tokenIt.IsEndOfStmtOrBlock())
                return stmt;

            var exp = _parser.ParseExpression(Terminators.ExpStatementEnd, passNewLine: false);
            stmt.Exp = exp;
            return stmt;
        }
    }



    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class ReturnExpr : Expr
    {
        /// <summary>
        /// Return value.
        /// </summary>
        public Expr Exp;


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object  DoEvaluate()
        {
            var parent = this.FindParent<FunctionExpr>();
            if (parent == null) throw new LangException("syntax error", "unable to return, parent not found", string.Empty, 0);

            object result = Exp == null ? null : Exp.Evaluate();
            bool hasReturnVal = Exp != null;
            parent.Return(result, hasReturnVal);
            return LNull.Instance;
        }
    }
}
