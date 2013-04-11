using System.Collections.Generic;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Core
{

    /* *************************************************************************
    <doc:example>	
    // Alias plugin is a base class for other plugin that want to register aliases
    // for tokens. e.g using "set" to actually represent "var" or using
    // "and" to represent &&
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class AliasTokenPlugin : TokenPlugin
    {
        private IDictionary<string, Token> _map = new Dictionary<string, Token>();


        /// <summary>
        /// Convenient access to token aliases map
        /// </summary>
        private IDictionary<string, Token> AliasMap
        {
            get { return _map; }
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public AliasTokenPlugin(string alias, Token replacement)
        {
            _tokens = new string[] { alias };
            _canHandleToken = true;
            Register(alias, replacement);
        }        


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "<alias> -> <replacement>";
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
                    "see examples for <alias>"
                };
            }
        }


        /// <summary>
        /// Register an alias.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacement"></param>
        public void Register(string text, Token replacement)
        {
            AliasMap[text] = replacement;
        }


        /// <summary>
        /// Whether or not this plugin is a match for the token supplied.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            if (_parser.Context.Symbols.Contains(current.Text))
                return false;
            return AliasMap.ContainsKey(current.Text);
        }


        /// <summary>
        /// Whether or not this plugin can handle the current token supplied which may be the current token
        /// or the next token.
        /// </summary>
        /// <param name="token">Current or Next token</param>
        /// <param name="isCurrent">Indicates if token supplied is the current token</param>
        /// <returns></returns>
        public override bool CanHandle(Token token, bool isCurrent)
        {
            if (token.Kind != TokenKind.Ident)
                return false;

            if (_parser.Context.Symbols.Contains(token.Text))
                return false; 
            return AliasMap.ContainsKey(token.Text);
        }


        /// <summary>
        /// Peeks at the token.
        /// </summary>
        /// <returns></returns>
        public override Token Peek()
        {
            var token = AliasMap[_tokenIt.NextToken.Token.Text];
            return token;
        }


        /// <summary>
        /// Parses the day expression.
        /// Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        /// </summary>
        /// <returns></returns>
        public override Token Parse()
        {
            var token = AliasMap[_tokenIt.NextToken.Token.Text];
            return token;
        }


        /// <summary>
        /// Parse the expression with parameters for moving the token iterator forward first
        /// </summary>
        /// <param name="advanceFirst">Whether or not to move the token iterator forward first</param>
        /// <param name="advanceCount">How many tokens to move the token iterator forward by</param>
        /// <returns></returns>
        public override Token Parse(bool advanceFirst, int advanceCount)
        {
            if (advanceFirst)
                _tokenIt.Advance(advanceCount);

            var token = AliasMap[_tokenIt.NextToken.Token.Text];
            return token;
        }
    }
}
