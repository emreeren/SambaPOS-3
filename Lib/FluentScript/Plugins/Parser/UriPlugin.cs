using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
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
    
    // Since this file has a space in it... you have to surround in quotes.
    var file3 = 'c:/data/blog posts.xml';
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling boolean values in differnt formats (yes, Yes, no, No, off Off, on On).
    /// </summary>
    public class UriPlugin : LexPlugin
    {
        private static Dictionary<string, string> _keywords = new Dictionary<string, string>();


        /// <summary>
        /// Initialize keywords for lookup.
        /// </summary>
        static UriPlugin()
        {
            _keywords["http"] = "http";
            _keywords["https"] = "httpS";
            _keywords["ftp"] = "ftp";
            _keywords["www"] = "www";
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public UriPlugin()
        {
            _tokens = new string[] { "http", "https", "ftp", "www", "$IdToken" };
            _canHandleToken = true;
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
            char n = _lexer.State.CurrentChar;
            char n2 = _lexer.PeekChar();
            
            // c:\folder\file.txt
            // c:/folder/file.txt
            if (n == ':' && (n2 == '/' || n2 == '\\'))
                return true;
                        
            // http https ftp ftps www
            if (!_keywords.ContainsKey(current.Text))
                return false;

            if (n == ':' || (n == '.' && string.Compare(current.Text, "www", StringComparison.InvariantCultureIgnoreCase) == 0))
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
            var line = _lexer.LineNumber;
            var pos = _lexer.LineCharPos;
            var letter = _lexer.State.CurrentChar;
            var lineTokenPart = _lexer.ReadUri();
            var finalText = takeoverToken.Token.Text + lineTokenPart.Text;
            var lineToken = ComLib.Lang.Tokens.ToLiteralString(finalText);
            var t = new TokenData() { Token = lineToken, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(t);
            return new Token[] { lineToken };
        }
    }
}
