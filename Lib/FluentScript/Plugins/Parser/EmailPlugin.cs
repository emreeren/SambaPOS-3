using System.Text.RegularExpressions;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Email plugin enables recognition of emails as strings without using quotes,
    // such as john.doe@company.com
    
    email1 = john.doe@company.com
    email2 = batman2012@gotham.com
    email3 = super.man_1@metropolis.com
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin allows emails without quotes such as john.doe@company.com
    /// </summary>
    public class EmailPlugin : LexPlugin
    {
        private const string _emailRegex = "^([0-9a-zA-Z]+[-._+&])*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$";


        /// <summary>
        /// Initialize
        /// </summary>
        public EmailPlugin()
        {
            _tokens = new string[] { "$IdToken" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "not available";
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
                    "john.doe@companyabc.com",
                    "bat.man_2012@gotham.com",
                    "super_man@metropolis.com"
                };
            }
        }


        /// <summary>
        /// Whether or not this uri plugin can handle the current token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var currentWord = _lexer.LastToken.Text;

            var peekResult = _lexer.Scanner.PeekCustomLimitedWord(false, '@', 25, '@', '.');
            if (!peekResult.Success) return false;

            var possibleEmail = currentWord + peekResult.Text;
            if (Regex.IsMatch(possibleEmail, _emailRegex))
                return true;
            return false;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            // http https ftp ftps www 
            var takeoverToken = _lexer.LastTokenData;
            var line = _lexer.State.Line;
            var pos = _lexer.State.LineCharPosition;
            var lineTokenPart = _lexer.ReadCustomWord('@', '.');
            var finalText = takeoverToken.Token.Text + lineTokenPart.Text;
            var lineToken = TokenBuilder.ToLiteralString(finalText);
            var t = new TokenData() { Token = lineToken, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(t);
            return new Token[] { lineToken };
        }
    }
}
