using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser.PluginSupport
{
    /// <summary>
    /// A combinator to extend the parser
    /// </summary>
    public class TokenPlugin : ITokenPlugin
    {
        /// <summary>
        /// Id of plugin.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// The starting tokens that are associated w/ the combinator.
        /// </summary>
        protected string[] _tokens;


        /// <summary>
        /// The core parser.
        /// </summary>
        protected Parser _parser;


        /// <summary>
        /// The token iterator
        /// </summary>
        protected TokenIterator _tokenIt;


        /// <summary>
        /// Whether or not this combinator can be made into a statemnt.
        /// </summary>
        protected bool _canHandleToken = false;


        /// <summary>
        /// Initialize the token plugin.
        /// </summary>
        public TokenPlugin()
        {
            this.Id = "Fluentscript.Lib." + this.GetType().Name.Replace("Plugin", string.Empty);
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
        /// The token iterator.
        /// </summary>
        public TokenIterator TokenIt { get { return _tokenIt; } set { _tokenIt = value; } }


        /// <summary>
        /// Precendence
        /// </summary>
        public int Precedence { get; set; }


        /// <summary>
        /// The tokens that are associated w/ this combinator.
        /// </summary>
        public virtual string[] StartTokens
        {
            get { return _tokens; }
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
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public virtual bool CanHandle(Token current)
        {
            return _canHandleToken;
        }


        /// <summary>
        /// Whether or not this plugin can handle the current token supplied which may be the current token
        /// or the next token.
        /// </summary>
        /// <param name="token">Current or Next token</param>
        /// <param name="isCurrent">Indicates if token supplied is the current token</param>
        /// <returns></returns>
        public virtual bool CanHandle(Token token, bool isCurrent)
        {
            return _canHandleToken;
        }


        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <returns></returns>
        public virtual Token Parse()
        {
            return null;
        }


        /// <summary>
        /// Parse the expression with parameters for moving the token iterator forward first
        /// </summary>
        /// <param name="advanceFirst">Whether or not to move the token iterator forward first</param>
        /// <param name="advanceCount">How many tokens to move the token iterator forward by</param>
        /// <returns></returns>
        public virtual Token Parse(bool advanceFirst, int advanceCount)
        {
            return null;
        }


        /// <summary>
        /// Peeks at the token and gets the replacement.
        /// </summary>
        /// <returns></returns>
        public virtual Token Peek()
        {
            return null;
        }
    }
}
