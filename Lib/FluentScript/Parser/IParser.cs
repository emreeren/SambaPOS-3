using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser
{
    /// <summary>
    /// Interface for the language parser. This is reused for combinators and the core parser.
    /// </summary>
    public interface ILangParser
    {
        /// <summary>
        /// The token iterator.
        /// </summary>
        TokenIterator TokenIt { get; }
    }



    /// <summary>
    /// A setup plugin to just configure the interpreter
    /// </summary>
    public interface ISetupPlugin
    {
        /// <summary>
        /// The id of the plugin.
        /// </summary>
        string Id { get; }


        /// <summary>
        /// Used for ordering of plugins.
        /// </summary>
        int Precedence { get; }


        /// <summary>
        /// Executes the configuration
        /// </summary>
        void Setup(Context ctx);
    }



    /// <summary>
    /// Marker interface for any type of plugin.
    /// </summary>
    public interface ILangPlugin
    {
        /// <summary>
        /// The id of the plugin.
        /// </summary>
        string Id { get; }


        /// <summary>
        /// Used for ordering of plugins.
        /// </summary>
        int Precedence { get; }


        /// <summary>
        /// Grammer for this plugin
        /// </summary>
        string Grammer { get; }


        /// <summary>
        /// Examples of grammer
        /// </summary>
        string[] Examples { get; }


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        bool CanHandle(Token current);
    }



    /// <summary>
    /// Interface for a plugin at the lexing level.
    /// </summary>
    public interface ILexPlugin : ILangPlugin
    {
        /// <summary>
        /// The lexer.
        /// </summary>
        Lexer Lexer { get; set; }


        /// <summary>
        /// Initialize the combinator.
        /// </summary>
        /// <param name="lexer">The main lexer</param>
        void Init(Lexer lexer);


        /// <summary>
        /// The tokens that are associated w/ this combinator.
        /// </summary>
        string[] StartTokens { get; }


        Context Ctx { get; set; }


        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <returns></returns>
        Token[] Parse();
    }



    /// <summary>
    /// Interface for plugin that handles token after lexical analysis but before parsing of expressions.
    /// </summary>
    public interface ITokenPlugin : ILangPlugin
    {
        /// <summary>
        /// Initialize the combinator.
        /// </summary>
        /// <param name="parser">The core parser</param>
        /// <param name="tokenIt">The token iterator</param>
        void Init(Parser parser, TokenIterator tokenIt);


        /// <summary>
        /// The tokens that are associated w/ this combinator.
        /// </summary>
        string[] StartTokens { get; }


        /// <summary>
        /// Whether or not this plugin can handle the supplied token.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isCurrentToken"></param>
        /// <returns></returns>
        bool CanHandle(Token token, bool isCurrentToken);


        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <returns></returns>
        Token Parse();


        /// <summary>
        /// Parse the expression with parameters for moving the token iterator forward first
        /// </summary>
        /// <param name="advanceFirst">Whether or not to move the token iterator forward first</param>
        /// <param name="advanceCount">How many tokens to move the token iterator forward by</param>
        /// <returns></returns>
        Token Parse(bool advanceFirst, int advanceCount);


        /// <summary>
        /// Peeks at the token without advancing the token iterator to the next token.
        /// </summary>
        /// <returns></returns>
        Token Peek();
    }



    /// <summary>
    /// Interface for a plugin at the parser/expression level.
    /// </summary>
    public interface IExprPlugin: ILangParser, ILangPlugin
    {
        /// <summary>
        /// Initialize the combinator.
        /// </summary>
        /// <param name="parser">The core parser</param>
        /// <param name="tokenIt">The token iterator</param>
        void Init(Parser parser, TokenIterator tokenIt);

        /// <summary>
        /// Expression parser.
        /// </summary>
        ExprParser ExpParser { get; set; }

        /// <summary>
        /// The context of the interpreter.
        /// </summary>
        Context Ctx { get; set; }


        /// <summary>
        /// Whether or not this combinator can be made into a statement.
        /// </summary>
        bool IsStatement { get; }


        /// <summary>
        /// Whether or not this is a system level plugin.
        /// </summary>
        bool IsSystemLevel { get; }


        /// <summary>
        /// Whether or not assignment is supported by this plugin.
        /// </summary>
        bool IsAssignmentSupported { get; }
        

        /// <summary>
        /// Whether or not a termninator is supported
        /// </summary>
        bool IsEndOfStatementRequired { get; }


        /// <summary>
        /// Whether or not the plugin auto handles if start tokens match.
        /// </summary>
        bool IsAutoMatched { get; set; }


        /// <summary>
        /// Grammar for matching the plugin.
        /// </summary>
        string GrammarMatch { get; set; }


        /// <summary>
        /// The tokens starting the expression.
        /// </summary>
        string[] StartTokens { get; }


        /// <summary>
        /// Parses an expression
        /// </summary>
        /// <returns></returns>
        Expr Parse();


        /// <summary>
        /// Parses an expression using the contextual object supplied
        /// </summary>
        /// <param name="context">Contextual informtion for parsing.</param>
        /// <returns></returns>
        Expr Parse(object context);
    }

    /// <summary>
    /// Marker interface for statements, expression plugins to recieve callbacks when parsing is complete.
    /// </summary>
    public interface IParserCallbacks
    {
        /// <summary>
        /// Called by the framework after the parse method is called
        /// </summary>
        /// <param name="node">The node returned by this implementations Parse method</param>
        void OnParseComplete(AstNode node);
    }
}
