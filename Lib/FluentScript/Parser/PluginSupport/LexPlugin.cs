using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
// </lang:using>

namespace ComLib.Lang.Parsing
{
    /// <summary>
    /// A combinator to extend the parser
    /// </summary>
    public class LexPlugin : ILexPlugin
    {
        /// <summary>
        /// Id of the plugin.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// The starting tokens that are associated w/ the combinator.
        /// </summary>
        protected string[] _tokens;


        /// <summary>
        /// The main lexer.
        /// </summary>
        protected Lexer _lexer;


        /// <summary>
        /// Whether or not this combinator can be made into a statemnt.
        /// </summary>
        protected bool _canHandleToken = false;


        /// <summary>
        /// Initialize the lexical plugin
        /// </summary>
        public LexPlugin()
        {
            this.Id = "ComLib." + this.GetType().Name.Replace("Plugin", string.Empty);
        }


        /// <summary>
        /// Initialize the combinator.
        /// </summary>
        /// <param name="lexer">The main lexer</param>
        public virtual void Init(Lexer lexer)
        {
            _lexer = lexer;
        }


        /// <summary>
        /// Precendence
        /// </summary>
        public int Precedence { get; set; }


        /// <summary>
        /// get / set the lexer.
        /// </summary>
        public virtual Lexer Lexer
        {
            get { return _lexer; }
            set { _lexer = value; }
        }


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
        /// Parses the expression.
        /// </summary>
        /// <returns></returns>
        public virtual Token[] Parse()
        {
            return null;
        }
    }
}
