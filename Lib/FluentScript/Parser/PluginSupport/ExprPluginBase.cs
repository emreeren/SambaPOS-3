using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser.PluginSupport
{
    /// <summary>
    /// A combinator to extend the parser
    /// </summary>
    public class ExprPluginBase
    {
        /// <summary>
        /// Used to uniquely identify a plugin.
        /// </summary>
        protected string _id;


        /// <summary>
        /// The token iterator
        /// </summary>
        protected TokenIterator _tokenIt;


        /// <summary>
        /// Tokens to handle the expression.
        /// </summary>
        protected string[] _startTokens;


        /// <summary>
        /// The core parser.
        /// </summary>
        protected Parser _parser;


        /// <summary>
        /// Whether or not to handle a new line as end of this expression plugin and for statement support.
        /// </summary>
        protected bool _handleNewLineAsEndOfExpression = false;


        /// <summary>
        /// The token iterator.
        /// </summary>
        public TokenIterator TokenIt { get { return _tokenIt; } set { _tokenIt = value; } }


        /// <summary>
        /// Initialize
        /// </summary>
        public ExprPluginBase()
        {
            this._id = "Fluentscript.Lib." + this.GetType().Name.Replace("Plugin", string.Empty);
        }


        /// <summary>
        /// Initialize the combinator.
        /// </summary>
        /// <param name="parser">The core parser</param>
        /// <param name="tokenIt">The token iterator</param>
        public virtual void Init(Parser parser, TokenIterator tokenIt)
        {
            _parser = parser;
            _tokenIt = tokenIt;
        }


        /// <summary>
        /// Configures this plugin as a system level statement.
        /// </summary>
        /// <param name="isCodeBlockSupported">Whether or not a code block is supported, e.g. if, while, for</param>
        /// <param name="isTerminatorSupported"></param>
        /// <param name="token"></param>
        public void ConfigureAsSystemStatement(bool isCodeBlockSupported, bool isTerminatorSupported, string token)
        {
            this.InitTokens(token);
            this.IsCodeBlockSupported = isCodeBlockSupported;
            this.IsStatement = true;
            this.IsSystemLevel = true;
            this.IsEndOfStatementRequired = isTerminatorSupported;
            this.IsAutoMatched = true;
        }


        /// <summary>
        /// Configures this plugin as a system level statement.
        /// </summary>
        /// <param name="isCodeBlockSupported">Whether or not a code block is supported, e.g. if, while, for</param>
        /// <param name="isTerminatorSupported"></param>
        /// <param name="token"></param>
        public void ConfigureAsSystemExpression(bool isCodeBlockSupported, bool isTerminatorSupported, string token)
        {
            this.InitTokens(token);
            this.IsCodeBlockSupported = isCodeBlockSupported;
            this.IsStatement = false;
            this.IsSystemLevel = true;
            this.IsEndOfStatementRequired = isTerminatorSupported;
            this.IsAutoMatched = true;
        }


        /// <summary>
        /// Gets the id for this plugin.
        /// </summary>
        public string Id { get { return _id; } }


        /// <summary>
        /// A number given to each plugin to give it an ordering compared to other plugins.
        /// </summary>
        public int Precedence { get; set; }


        /// <summary>
        /// Whether or not this grammer is context free grammer.
        /// </summary>
        public bool IsContextFree { get; set; }


        /// <summary>
        /// Whether or not this expression can be used like a statement.
        /// </summary>
        public bool IsStatement { get; set; }

        
        /// <summary>
        /// Whether or not this is a system level plugin
        /// </summary>
        public bool IsSystemLevel { get; set; }


        /// <summary>
        /// Whether or not this plugin supports assignments
        /// </summary>
        public bool IsAssignmentSupported { get; set; }


        /// <summary>
        /// Whether or not a codeblock is supported.
        /// </summary>
        public bool IsCodeBlockSupported { get; set; }


        /// <summary>
        /// Whether or not a terminator is supported.
        /// </summary>
        public bool IsEndOfStatementRequired { get; set; }


        /// <summary>
        /// Whether or not this plugin automatically takes over parsing on the match of it's start tokens.
        /// </summary>
        public bool IsAutoMatched { get; set; }

        /// <summary>
        /// The context of the script.
        /// </summary>
        public Context Ctx { get; set; }


        /// <summary>
        /// Grammar for matching the plugin.
        /// </summary>
        public string GrammarMatch { get; set; }


        /// <summary>
        /// The tokens that are associated w/ this combinator.
        /// </summary>
        public string[] StartTokens
        {
            get { return _startTokens; }
            set { _startTokens = value; }
        }


        /// <summary>
        /// Grammer for this plugin
        /// </summary>
        public virtual string Grammer
        {
            get { return string.Empty; }
        }


        /// <summary>
        /// Examples of grammer
        /// </summary>
        public virtual string[] Examples
        {
            get { return null; }
        }


        /// <summary>
        /// Whether or not to handle a new line as end of statement/expression.
        /// </summary>
        public virtual bool IsNewLineEndOfExpression
        {
            get { return _handleNewLineAsEndOfExpression; }
        }


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public virtual bool CanHandle(Token current)
        {
            return IsAutoMatched;
        }


        public ExprParser ExpParser { get; set; }


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


        private void InitTokens(string token)
        {
            if (!token.Contains(","))
            {
                this.StartTokens = new string[] { token };
                return;
            }
            var tokens = token.Split(',');
            this.StartTokens = tokens;
        }
    }
}
