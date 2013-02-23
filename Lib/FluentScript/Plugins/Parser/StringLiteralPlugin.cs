using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Helpers;
using ComLib.Lang.Parsing;
using ComLib.Lang.Types;
// </lang:using>


namespace ComLib.Lang.Plugins
{

    /* *************************************************************************
    <doc:example>	
    // Allows using ruby-style string literals such as :user01 where :user01 equals 'user01'
    
    name = :user01
    lang = :fluentscript
    
    if( :batman == 'batman' ) print( "works" );
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin allows emails without quotes such as john.doe@company.com
    /// </summary>
    public class StringLiteralPlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public StringLiteralPlugin()
        {
            this.Precedence = 1;
            this.IsAutoMatched = true;
            this.StartTokens = new string[] { ":" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return ":username";
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
                    "var name = :kishore",
                    "var lang = :fluent_script"
                };
            }
        }


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool CanHandle(Token token)
        {
            var next = _tokenIt.Peek();
            if (next.Token.Kind == TokenKind.Ident) 
                return true;
            return false;
        }


        /// <summary>
        /// Parses the day expression.
        /// Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var startToken = _tokenIt.NextToken;
            _tokenIt.Advance();
            var word = _tokenIt.NextToken.Token.Text;
            _tokenIt.Advance();
            return Exprs.Const(new LString(word), startToken);
        }
    }
}
