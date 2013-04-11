using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser
{
    /// <summary>
    /// Base class for the parser
    /// </summary>
    public class ParserBase : ILangParser
    {
        #region Protected members
        /// <summary>
        /// Scope of the script
        /// </summary>
        protected Memory _memory = null;


        /// <summary>
        /// Context information about the script.
        /// </summary>
        protected Context _context = null;


        /// <summary>
        /// Lexer to parse tokens.
        /// </summary>
        protected Lexer _lexer = null;


        /// <summary>
        /// The script as text.
        /// </summary>
        protected string _script;


        /// <summary>
        /// The path to the script if script was provided as a file path instead of text
        /// </summary>
        protected string _scriptPath;


        /// <summary>
        /// The parsed statements from interpreting the tokens.
        /// </summary>
        protected List<Expr> _statements; 


        /// <summary>
        /// The state of the parser .. used in certain cases.
        /// </summary>
        protected ParserState _state;


        /// <summary>
        /// Settings of the lanaguage interpreter.
        /// </summary>
        protected LangSettings _settings;


        /// <summary>
        /// Token iterator.
        /// </summary>
        protected TokenIterator _tokenIt;


        /// <summary>
        /// List of the last doc tags.
        /// </summary>
        protected List<Token> _comments;
        
        
        /// <summary>
        /// Whether or not there are function summary doc tags in the stack.
        /// </summary>
        protected bool _hasSummaryComments = false;
        
        
        /// <summary>
        /// The last summary doc tag token.
        /// </summary>
        protected TokenData _lastCommentToken;


        /// <summary>
        /// Collection of errors from parsing.
        /// </summary>
        protected List<LangException> _parseErrors;


        /// <summary>
        /// Get the list of parsed statements.
        /// </summary>
        internal List<Expr> Statements { get { return _statements; } }
        #endregion


        /// <summary>
        /// Initialize
        /// </summary>
        public ParserBase(Context context)
        {
            _context = context;
            _parseErrors = new List<LangException>();
            _lexer = new Lexer("");
            _lexer.SetContext(_context); 
        }


        #region Public properties
        /// <summary>
        /// Get the scope
        /// </summary>
        public Memory Scope { get { return _memory; } }


        /// <summary>
        /// Get/Set the context of the script.
        /// </summary>
        public Context Context { get { return _context; } set { _context = value; _memory = _context.Memory; } }


        /// <summary>
        /// Name of the current script being parsed.
        /// Set from the Interpreter object.
        /// </summary>
        public string ScriptName { get; set; }


        /// <summary>
        /// Get the lexer.
        /// </summary>
        public Lexer Lexer { get { return _lexer; } }


        /// <summary>
        /// Settings
        /// </summary>
        public LangSettings Settings { get { return _settings; } set { _settings = value; } }


        /// <summary>
        /// The token iterator.
        /// </summary>
        public TokenIterator TokenIt { get { return _tokenIt; } }


        /// <summary>
        /// Get the parser state.
        /// </summary>
        public ParserState State { get { return _state; } }


        /// <summary>
        /// The path to the script
        /// </summary>
        public string ScriptPath { get { return _scriptPath; } }


        
        #endregion


        #region Public methods
        /// <summary>
        /// Intializ with script and optional memory object for reruns.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="memory"></param>
        public virtual void Init(string script, Memory memory)
        {
            _script = script;
            _scriptPath = string.Empty;
            _statements = new List<Expr>();
            _memory = _memory == null ? new Memory() : memory;
            _lexer.Init(script);
            _parseErrors.Clear();
            _state = new ParserState(); 
           
            if (_comments != null) _comments.Clear();
            else _comments = new List<Token>();
        }


        /// <summary>
        /// Collects an unexpected token error and advances to next token.
        /// </summary>
        public void CollectError()
        {
            // Should not be here if script ended.
            if (_tokenIt.IsEnded)
                throw _tokenIt.BuildEndOfScriptException();

            // Store the error.
            var ex = _tokenIt.BuildSyntaxUnexpectedTokenException();
            this._parseErrors.Add(ex);

            // Now advance to next token to continue parsing.
            _tokenIt.Advance();
        }


        /// <summary>
        /// Collects an unexpected token error and advances to next token.
        /// </summary>
        public void AddError(TokenData token, string error)
        {
            var ex = new LangException("Parse", error, this._scriptPath, token.Line, token.LineCharPos);
            this._parseErrors.Add(ex);
        }
        #endregion


        #region Helpers
        /// <summary>
        /// Convert the script into a series of tokens.
        /// </summary>
        protected void Tokenize()
        {
            _lexer.IntepolatedStartChar = _settings.InterpolatedStartChar;
            _tokenIt.Init((llk) => _lexer.GetTokenBatch(llk), 12, null);
        }        


        /// <summary>
        /// End of statement script.
        /// </summary>
        /// <param name="endOfStatementToken"></param>
        /// <returns></returns>
        protected bool IsEndOfStatementOrEndOfScript(Token endOfStatementToken)
        {
            // Copied code... to avoid 2 function calls.
            if (_tokenIt.NextToken.Token == endOfStatementToken) return true;
            if (_tokenIt.NextToken.Token == Tokens.EndToken) return true;
            return false;
        }


        /// <summary>
        /// Whether at end of statement.
        /// </summary>
        /// <returns></returns>
        protected bool IsEndOfStatement(Token endOfStatementToken)
        {
            return (_tokenIt.NextToken.Token == endOfStatementToken);
        }


        /// <summary>
        /// Whether at end of script
        /// </summary>
        /// <returns></returns>
        protected bool IsEndOfScript()
        {
            return _tokenIt.NextToken.Token == Tokens.EndToken;
        }


        /// <summary>
        /// Parses a sequence of names/identifiers.
        /// </summary>
        /// <returns></returns>
        public List<string> ParseNames()
        {
            var names = new List<string>();
            while (true)
            {
                // Case 1: () empty list
                if (IsEndOfStatementOrEndOfScript(Tokens.RightParenthesis))
                    break;

                // Skip new lines.
                _tokenIt.AdvancePastNewLines();

                // Case 2: name and auto-advance to next token
                var name = _tokenIt.ExpectId(true);
                names.Add(name);

                // Case 3: only 1 argument. 
                if (IsEndOfStatementOrEndOfScript(Tokens.RightParenthesis))
                    break;

                // Skip new lines.
                _tokenIt.AdvancePastNewLines();

                // Case 4: comma, more names to come
                if (_tokenIt.NextToken.Token == Tokens.Comma) _tokenIt.Advance();
            }
            return names;
        }
        #endregion


        #region Comment Handling
        /// <summary>
        /// Handles a comment token.
        /// </summary>
        /// <param name="tokenData"></param>
        /// <param name="token"></param>
        protected void HandleComment(TokenData tokenData, Token token)
        {
            var text = token.Text;
            // Case 1: Summary tag for functions or scriptmeta tag for script info
            if (text.Contains("@summary") || text.Contains(" @summary")
                || text.Contains("@scriptmeta-start") || text.Contains(" @scriptmeta-start"))
            {
                _hasSummaryComments = true;
                _lastCommentToken = tokenData;
            }
            // Case 2: 
            else if (text.Contains("@scriptmeta-end") || text.Contains(" @scriptmeta-end"))
            {
                this.ClearCommentHandling();
            }
            if (_hasSummaryComments)
                _comments.Add(token);

            // Finally advance the token.
            _tokenIt.Advance();
        }


        /// <summary>
        /// Applies the last doc tags to the function statement.
        /// </summary>
        /// <param name="stmt"></param>
        protected void ApplyDocTagsToFunction(Expr stmt)
        {
            if (!_hasSummaryComments) return;
            if (!(stmt.IsNodeType(NodeTypes.SysFunctionDeclare)))
            {
                throw _tokenIt.BuildSyntaxUnexpectedTokenException(_lastCommentToken);
            }

            // Get the function associated w/ the declaration.
            // Parse the doc tags.
            // Apply the doc tags to the function.
            var func = ((FunctionDeclareExpr)stmt).Function;
            var tags = DocHelper.ParseDocTags(_comments);
            func.Meta.Doc = tags.Item1;
            
            // Associate all the argument specifications to the function metadata
            foreach (var arg in tags.Item1.Args)
            {
                if (string.IsNullOrEmpty(arg.Name))
                    continue;

                if (!func.Meta.ArgumentsLookup.ContainsKey(arg.Name))
                {
                    //_tokenIt.BuildSyntaxException("Doc argument name : '" + arg.Name + "' does not exist in function : " + func.Meta.Name);
                    // TODO: Create warning.
                }
                else
                {
                    var funcArg = func.Meta.ArgumentsLookup[arg.Name];
                    funcArg.Alias = arg.Alias;
                    funcArg.Desc = arg.Desc;
                    funcArg.Examples = arg.Examples;
                    funcArg.Type = arg.Type;

                    // Now associate the alias to the arg names.
                    if (!string.IsNullOrEmpty(funcArg.Alias))
                    {
                        func.Meta.ArgumentsLookup[funcArg.Alias] = funcArg;
                        func.Meta.ArgumentNames[funcArg.Alias] = funcArg.Alias;
                    }
                }
            }

            this.ClearCommentHandling();
        }


        private void ClearCommentHandling()
        {
            // Clear the comment state.
            _comments.Clear();
            _hasSummaryComments = false;
            _lastCommentToken = null;
        }
        #endregion


        #region Token methods
        /// <summary>
        /// Match the current token to the token supplied.
        /// </summary>
        /// <param name="count">The number of positions to move forward</param>
        /// <param name="passNewLine">Whether or not to pass a new line token</param>
        protected void Advance(int count = 1, bool passNewLine = true)
        {
            _tokenIt.Advance(count, passNewLine);
        }


        /// <summary>
        /// Match the current token to the token supplied.
        /// </summary>
        /// <param name="token">The token to match the current token against</param>
        protected void Expect(Token token)
        {
            _tokenIt.Expect(token);
        }


        /// <summary>
        /// Match the current token to the token supplied.
        /// </summary>
        /// <param name="token1">The first token to expect</param>
        /// <param name="token2">The second token to expect</param>
        /// <param name="token3">The third token to expect</param>
        protected void ExpectMany(Token token1, Token token2 = null, Token token3 = null)
        {
            _tokenIt.ExpectMany(token1, token2, token3);
        }


        /// <summary>
        /// Peek at the token ahead of the current token
        /// </summary>
        /// <param name="count">The number of tokens ahead to peek at</param>
        /// <param name="passNewLine">Whether or not the pass a new line token when peeking</param>
        protected TokenData Peek(int count = 1, bool passNewLine = false)
        {
            return _tokenIt.Peek(1, passNewLine);
        }
        #endregion
    }
}
