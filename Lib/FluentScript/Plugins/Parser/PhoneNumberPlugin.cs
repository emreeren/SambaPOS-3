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
    // Uri plugin allows you urls and file paths without surrounding them in 
    // quotes as long as there are no spaces. These are interpreted as strings.
    
    var url1 = www.yahoo.com;
    var url2 = http://www.google.com;
    var url3 = http://www.yahoo.com?user=kishore%20&id=123;
    var file1 = c:\users\kishore\settings.ini;
    var file2 = c:/data/blogposts.xml;
    var printer = \\printnetwork1\printer1
    
    // Since this file has a space in it... you have to surround in quotes.
    var file3 = 'c:/data/blog posts.xml';
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling boolean values in differnt formats (yes, Yes, no, No, off Off, on On).
    /// </summary>
    public class PhoneNumberPlugin : LexPlugin
    {
        private static Dictionary<string, string> _keywords = new Dictionary<string, string>();

        
        /// <summary>
        /// Initialize
        /// </summary>
        public PhoneNumberPlugin()
        {
            _tokens = new string[] { "$NumberToken" }; 
            _canHandleToken = true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "[0-9]{3}-[0-9]3-[0-9]{4}";
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
                    "800-410-1234"
                };
            }
        }


        /// <summary>
        /// Whether or not this uri plugin can handle the current token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool  CanHandle(Token current)
        {   
            // Check 1: 3 digit area code.
            // Only supporting u.s. numbers for now which require area code
            // area code must be 3 digits
            if (current.Text.Length > 3)
                return false;

            var n = _lexer.State.CurrentChar;

            // Check 2: only support "-" between area code and next set of numbers.
            if(n != '-')
                return false;

            var n2 = _lexer.Scanner.PeekMaxChars(8);
            var isMatch = Regex.IsMatch(n2, @"\d{3}[-]\d{4}");
            return isMatch;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            // Last token: number
            var takeoverToken = _lexer.LastTokenData;
            var line = _lexer.State.Line;
            var pos = _lexer.State.LineCharPosition;

            // Current char is "-"
            var letter = _lexer.State.CurrentChar;

            // Get next 8 chars
            // TODO: Optimize here ( although after move to metacompiler lexical plugins optimizations may not be needed )
            var phoneText = _lexer.Scanner.PeekMaxChars(8);
            _lexer.Scanner.MoveChars(9);

            var finalText = takeoverToken.Token.Text + letter + phoneText;
            var lineToken = TokenBuilder.ToLiteralString(finalText);
            var t = new TokenData() { Token = lineToken, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(t);
            return new Token[] { lineToken };
        }
    }
}
