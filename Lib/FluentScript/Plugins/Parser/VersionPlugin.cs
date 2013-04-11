using System.Collections.Generic;
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
    // Version plugin enables representation of versions using format 1.2.3.4.
    // This is particularily useful for when fluentscript is used for build automation.
    // e.g. 0.9.8.7
    
    version  = 0.9.8.7
    version2 = 0.9.8
    
    print( version.Major )
    print( version.Minor )
    print( version.Revision )
    print( version.Build )
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin allows emails without quotes such as john.doe@company.com
    /// </summary>
    public class VersionPlugin : LexPlugin
    {
        private const string _versionRegex = "^[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}(\\.[0-9]{1,4})?";
        private static IDictionary<char, bool> _numbers = new Dictionary<char, bool>()
        {
            { '0', true},
            { '1', true},
            { '2', true},
            { '3', true},
            { '4', true},
            { '5', true},
            { '6', true},
            { '7', true},
            { '8', true},
            { '9', true},
            { '.', true}             
        };

        /// <summary>
        /// Initialize
        /// </summary>
        public VersionPlugin()
        {
            _tokens = new string[] { "$NumberToken" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}(\\.[0-9]{1,4})?";
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
                    "0.9.8.7",
                    "2.9.3.355",
                    "1.2.8"                    
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
            // Given 2.1.5.82
            // Current = 2.1
            if (_lexer.State.CurrentChar != '.') return false;
            var result = _lexer.Scanner.PeekCustomWord(_numbers, false);
            if (!result.Success) return false;

            var versionText = current.Text + result.Text;
            if (Regex.IsMatch(versionText, _versionRegex))
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

            _lexer.Scanner.ReadChar();
            var token = _lexer.ReadNumber();
            var finalText = takeoverToken.Token.Text + "." + token.Text;
            var lineToken = TokenBuilder.ToLiteralVersion(finalText);
            var t = new TokenData() { Token = lineToken, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(t);
            return new Token[] { lineToken };
        }
    }
}
