using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang
{

    /* *************************************************************************
    <doc:example>	
    // Throw plugin provides throwing of errors from the script.
    
    throw 'user name is required';
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin for throwing errors from the script.
    /// </summary>
    public class ThrowPlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public ThrowPlugin()
        {
            this.ConfigureAsSystemStatement(false, true, "throw");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "throw <expression> <statementterminator>"; }
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
                    "throw 'invalid amount';",
                    "throw 300\r\n"
                };
            }
        }


        /// <summary>
        /// throw error;
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
             _tokenIt.Expect(Tokens.Throw);
            var exp = _parser.ParseExpression(Terminators.ExpStatementEnd, passNewLine: true);
            return new ThrowExpr() { Exp = exp };
        }
    }



    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class ThrowExpr : Expr
    {
        /// <summary>
        /// Create new instance
        /// </summary>
        public ThrowExpr()
        {
        }


        /// <summary>
        /// Name for the error in the catch clause.
        /// </summary>
        public Expr Exp;


        /// <summary>
        /// Execute
        /// </summary>
        public override object DoEvaluate()
        {
            object message = null;
            if (Exp != null)
                message = Exp.Evaluate();

            throw new LangException("TypeError", message.ToString(), this.Ref.ScriptName, this.Ref.Line);
        }
    }
}
