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
    public class PreprocessorPlugin : LexPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public PreprocessorPlugin()
        {
            _tokens = new string[] { "@" };
            _canHandleToken = false;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return " ( ( http | https | ftp | www )) ':' ( [a-zA-Z0-9] | [^' ' \t '(' ')' ';' ',' '[' ']' ] )* "
                       + " | ( <id> ':' '\' '\' ( [a-zA-Z0-9] | [^' ' \t '(' ')' ';' ',' '[' ']' ] )* ) )";
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
                    @"www.yahoo.com",
                    @"http://www.google.com",
                    @"http://www.yahoo.com?user=kishore%20&id=123",
                    @"c:\users\kishore\settings.ini",
                    @"c:/data/blogposts.xml"
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
            var n1 = _lexer.PeekToken(false);
            if (n1.Token.Text == "if" || n1.Token.Text == "endif")
                return true;
            return false;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            // 1. Current token is "@"
            var resultTokens = new Token[] {Tokens.Ignore};

            // 2. Move past if
            var word = _lexer.ReadWord();

            // 3. Is start of directive ?
            if (word.Text == "if")
            {
                // 1. Get whole line
                var line = _lexer.ReadLineRaw(false);
                var results = PreprocessHelper.Process(line.Text);

                // 2. Keep track of last directive.
                this.Ctx.Directives.StartDirectiveCode(results.Keys[0]);
                    
                // 3. If valid directive condition don't do anything.
                //    Allow the lexer to parse all the code as tokens inside the directive.
                if (results.IsTrue)
                {
                    return resultTokens;
                }

                // 4. if false... ignore the whole code until "@if"
                _lexer.SkipUntilPrefixedWord('@', "endif");
                return resultTokens;
            }

            // 3b: "endif"
            this.Ctx.Directives.EndDirectiveCode();
            return resultTokens;
        }
    }
}
