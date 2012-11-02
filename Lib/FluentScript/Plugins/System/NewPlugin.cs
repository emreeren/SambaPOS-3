using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang
{

    /* *************************************************************************
    <doc:example>	
    // Enables the use of new to create instances of objects.
    
    post = new BlogPost()
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin for throwing errors from the script.
    /// </summary>
    public class NewPlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public NewPlugin()
        {
            this.ConfigureAsSystemExpression(false, false, "new");
            this.Precedence = 100;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "new <expression>"; }
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
                    "new BlogPost()"
                };
            }
        }


        /// <summary>
        /// throw error;
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // Validate
            _tokenIt.Expect(Tokens.New);
            string typeName = _tokenIt.ExpectId();
            
            // Keep parsing to capture full name.
            // e.g new App.Core.User()
            while (_tokenIt.NextToken.Token == Tokens.Dot)
            {
                _tokenIt.Advance();
                string name = _tokenIt.ExpectId();
                typeName += "." + name;
                if (_tokenIt.IsEndOfStmtOrBlock())
                    break;
            }
            var exp = new NewExpr() { TypeName = typeName };
            _parser.State.FunctionCall++;
            _parser.ParseParameters(exp, true, false);
            _parser.State.FunctionCall--;
            return exp;
        }
    }
}
