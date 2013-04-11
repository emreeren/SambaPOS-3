using System;
using System.Collections.Generic;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>


namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Allows represnting file extension with out having to surround them in quotes
    // e.g. such as .doc, or *.doc 
    
    // Format: "*" "." <extension_name>
    // NOTE: The "*" is optional.
    
    some_file_operation( 'c:\\app\\src\\', .pdb )
    some_file_operation( 'c:\\app\\src\\', *.dll )
    some_file_operation( 'c:\\app\\src\\', .svn )
    some_file_operation( 'c:\\app\\src\\', .exe )
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin allows emails without quotes such as john.doe@company.com
    /// </summary>
    public class FileExtPlugin : LexPlugin
    {
        private string[] _extensions = null;
        private IDictionary<string, string> _extLookup;


        /// <summary>
        /// Initialize
        /// </summary>
        public FileExtPlugin()
        {
            _tokens = new string[] { "*", "." };
            _extensions = new string[] 
            {
	            "xml",  "js",   "dll",  "exe",
	            "doc",  "ppt",  "xls",
	            "txt",  "log",
                "svn", "cvs",
	            "cs",  "java",  "fs",  "aspx",  "ascx",  "rb",  "py",
	            "mp3",  "mp4",  "avi"
            };
            _extLookup = new Dictionary<string, string>();
            _extLookup = LangHelper.ToDictionary(_extensions);
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "'*'? '.' <word> ";
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
                    ".xml",
                    "*.xml",
                    ".doc"
                };
            }
        }


        /// <summary>
        /// Whether or not this uri plugin can handle the current token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool CanHandle(Token token)
        {
            var next = _lexer.Scanner.PeekChar();
            var curr = _lexer.State.CurrentChar;
            var last = ' ';
            if(_lexer.State.Pos - 2 >= 0)
                last = _lexer.State.Text[_lexer.State.Pos - 2];

            // [ ident.xml, 9.doc ]
            if (Char.IsLetterOrDigit(last)) return false;
            if (last == ')' || last == ']' ) return false;
            if (token.Text == "." && !Char.IsLetter(curr)) return false;

            // *.
            if (token == Tokens.Multiply && curr == '.')
            {
                var result = _lexer.Scanner.PeekWord(true);
                if (!result.Success) return false;
                if (_extLookup.ContainsKey(result.Text))
                    return true;
            }
            else if (token == Tokens.Dot)
            {
                var result = _lexer.Scanner.PeekWord(false);
                if (!result.Success) return false;
                if (_extLookup.ContainsKey(result.Text))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            // *.xml .xml .doc *.doc
            var takeoverToken = _lexer.LastTokenData;
            var pretext = takeoverToken.Token.Text;
            var line = _lexer.State.Line;
            var pos = _lexer.State.LineCharPosition;

            // 1. Check if starts with "*" in which case its really "*."
            if (_lexer.State.CurrentChar == '.')
            {
                _lexer.Scanner.ReadChar();
                pretext += ".";
            }

            // 2. Get the file extension name.
            var lineTokenPart = _lexer.ReadWord();
            var finalText = pretext + lineTokenPart.Text;
            var token = TokenBuilder.ToLiteralString(finalText);
            var t = new TokenData() { Token = token, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(t);
            return new Token[] { token };
        }
    }
}
