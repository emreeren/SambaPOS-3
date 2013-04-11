using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{
    /* *************************************************************************
    <doc:example>	
    // Marker plugin allows you to mark code like comments but in the form of statements
    // so that syntax is checked. This allows for structured comments.
    
    // Case1 : Todo
    @todo: i need to add extra comments here
    
    // Case2 : Todo quoted
    @todo: "I need to add extra checks here and
    also do additional cleanup of code"
    
    // Case 3 : bug
    @bug: 'this bug related to parsing'
    @bug: this bug related to johns code!
    
    </doc:example>
    ***************************************************************************/
    /// <summary>
    /// Combinator for handling boolean values in differnt formats (yes, Yes, no, No, off Off, on On).
    /// </summary>
    public class MarkerLexPlugin : LexPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public MarkerLexPlugin()
        {
            // "todo", "note", "bug", "review", "implement", "optimize", "refactor", "critical"
            _tokens = new string[] { "@" };
        }


        /// <summary>
        /// Whether or not the lexer can handle this token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var peekResult = _lexer.Scanner.PeekWord(false);
            if (peekResult.Success && MarkerPlugin._markers.ContainsKey(peekResult.Text))
                return true;

            return false;
        }


        /// <summary>
        /// todo i need
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            // print no quotes needed!
            var takeoverToken = _lexer.LastTokenData;
            int line = _lexer.State.Line;
            int pos = _lexer.State.LineCharPosition;

            // What is the next word?                        
            var marker = _lexer.ReadWord();
            var m = new TokenData() { Token = marker, Line = line, LineCharPos = pos };

            _lexer.Scanner.ReadChar();
            _lexer.Scanner.ConsumeWhiteSpace(false, true);
            Token token = null;
            line = _lexer.State.Line;
            pos = _lexer.State.LineCharPosition;            
            
            char c = _lexer.State.CurrentChar;
            if (c == '\'' || c == '"')
                token = _lexer.ReadInterpolatedString(c, false, false, true);
            else
                token = _lexer.ReadLine(false);

            var t = new TokenData() { Token = token, Line = line, LineCharPos = pos };
            _lexer.ParsedTokens.Add(takeoverToken);
            _lexer.ParsedTokens.Add(m);
            _lexer.ParsedTokens.Add(t);
            return new Token[] { takeoverToken.Token, token };
        }
    }



    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class MarkerPlugin : ExprPlugin
    {
        internal static Dictionary<string, string> _markers;


        /// <summary>
        /// Setup all the known markers.
        /// </summary>
        static MarkerPlugin()
        {
            _markers = new Dictionary<string, string>();
            _markers["todo"]        = "todo";
            _markers["note"]        = "note";       
            _markers["bug"]         = "bug";
            _markers["review"]      = "review";
            _markers["implement"]   = "implement";
            _markers["optimize"]    = "optimize";
            _markers["refactor"]    = "refactor";
            _markers["critical"]    = "critical";
        }
        

        /// <summary>
        /// Initialize
        /// </summary>
        public MarkerPlugin()
        {
            this.StartTokens = new string[] { "@" };
            this.IsStatement = true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "'@' ( todo | note | bug | review | implement | optimize | refactor | critical ) <line>";
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
                    "@todo: i need to add extra comments here",
                    "@todo: 'I need to add extra checks here and'",
                    "@bug: 'this bug related to parsing'",
                    "@bug: this bug related to johns code!"
                };
            }
        }


        /// <summary>
        /// Whether or not this plugin can handle the token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            // Syntax is @<marker>
            var n = _tokenIt.Peek().Token;
            if (!_markers.ContainsKey(n.Text))
                return false;

            return true;
        }


        /// <summary>
        /// Parses the marker into a MarkerStmt.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // @todo: 
            // 1. Move past "@"
            _tokenIt.Advance();
            
            // 2. Get the marker tag: todo, review, bug, etc and move past ":"
            var marker = _tokenIt.NextToken.Token.Text;
            _tokenIt.Advance();

            // 3. Literal string.
            var current = _tokenIt.NextToken.Token;
            _tokenIt.Advance();

            if (_tokenIt.IsExplicitEndOfStmt())
                _tokenIt.ExpectEndOfStmt();
            return new MarkerExpr(marker, current.Text);
        }
    }



    /// <summary>
    /// Stmt to represent the marker information.
    /// </summary>
    public class MarkerExpr : Expr
    {
        /// <summary>
        /// Initialize the marker.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="desc"></param>
        public MarkerExpr(string tag, string desc)
        {
            Tag = tag;
            Desc = desc;
        }


        /// <summary>
        /// The tag. e.g. todo, bug, note etc.
        /// </summary>
        public string Tag { get; set; }


        /// <summary>
        /// The description of the marker.
        /// </summary>
        public string Desc { get; set; }


        /// <summary>
        /// Stores the name of the person who made the marker.
        /// </summary>
        public string Author { get; set; }


        /// <summary>
        /// Priority of the marker
        /// </summary>
        public int Priority { get; set; }
    }
}