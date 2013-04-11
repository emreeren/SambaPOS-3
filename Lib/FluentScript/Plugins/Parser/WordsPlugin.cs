using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Words plugin enables the registration of custom text/words into the language
    // which are then immediately available for use. 
    // They are basically an alternative to not having to surround text in double quotes
    // Only 
    @words( nasdaq, fluent script )
    @words  IBM, MSFT, APPL
    
    if ( "fluent script" == fluent script ) then
        print( "works" )
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling swapping of variable values. swap a and b.
    /// </summary>
    public class WordsPlugin : ExprPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public WordsPlugin()
        {
            this.StartTokens = new string[] { "@" };
            this.IsStatement = true;
            this.IsEndOfStatementRequired = true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "@ words '(' <id>* ( ',' <id>* )* ')'";
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
                    "@words ( ibm, microsoft )",
                    "@words ( default pricing, premium pricing )"
                };
            }
        }


        /// <summary>
        /// Whether or not this can handle the token supplied.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var ahead = _tokenIt.Peek(1, false);
            if (string.Compare(ahead.Token.Text, "words", StringComparison.InvariantCultureIgnoreCase) == 0)
                return true;

            return false;
        }


        /// <summary>
        /// @addwords( nasdaq, fluent script )
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            _tokenIt.Advance(2, false);
            var hasParens = _tokenIt.NextToken.Token == Tokens.LeftParenthesis;
            if(hasParens)
                _tokenIt.Expect(Tokens.LeftParenthesis);
            var words = new List<string>();

            var token = _tokenIt.NextToken;

            // Loop through to register all words separated by comma.
            while (token.Token != Tokens.RightParenthesis && token.Token != Tokens.NewLine)
            {
                if (_tokenIt.IsEnded)
                    break;

                if (!hasParens && token.Token == Tokens.Semicolon)
                    break;

                string word = GetNextWord(hasParens);
                this._parser.Context.Words.Register(word);

                token = _tokenIt.Advance(1, false);
                if (token.Token == Tokens.Comma)
                    token = _tokenIt.Advance(1, false);
            }
            if(hasParens)
                _tokenIt.Expect(Tokens.RightParenthesis);
            
            return new Expr();
        }


        private string GetNextWord(bool hasParens)
        {
            var ahead = _tokenIt.Peek(1, false);
            var current = _tokenIt.NextToken;
            string word = current.Token.Text;
            
            // Build up the word until , is hit.
            while (ahead.Token != Tokens.Comma && ahead.Token != Tokens.RightParenthesis 
                && ahead.Token != Tokens.EndToken)
            {
                // Stop at new line if no parenthesis
                if (!hasParens && (ahead.Token == Tokens.NewLine || ahead.Token == Tokens.Semicolon))
                    break;

                current = _tokenIt.Advance(1, false);
                word += " " + current.Token.Text;
                ahead = _tokenIt.Peek(1, false);
            }
            return word;
        }        
    }



    /// <summary>
    /// Combinator for handling swapping of variable values. swap a and b.
    /// </summary>
    public class WordsInterpretPlugin : ExprPlugin
    {
        private static string[] _tokens = new string[] { "$IdToken" };
        private int _idCount = 0;
        private string _word;
        private List<string> _possibleWords;


        /// <summary>
        /// Intialize.
        /// </summary>
        public WordsInterpretPlugin()
        {
            _startTokens = _tokens;
            _possibleWords = new List<string>();
            Precedence = 1;
        }


        /// <summary>
        /// Whether or not this can handle the token supplied.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            _idCount = 0;
            _word = string.Empty;

            // Keep reading while word doesn't exist.
            var ahead = _tokenIt.Peek(1, false);
            var words = _parser.Context.Words;
            bool isWordReplacement = false;

            // Just 1 word check.
            if( ahead.Token.Kind != TokenKind.Ident || ahead.Token == Tokens.EndToken)
            {
                isWordReplacement = words.ContainsKey(current.Text);
                _word = current.Text;
                return isWordReplacement;
            }
            bool found = false;
            _possibleWords = _tokenIt.PeekConsequetiveIdsAppended(_tokenIt.LLK);

            // Do a reverse check.
            // 1. Get consequetive id tokens.
            // e.g. default premium policy
            for (int ndx = _possibleWords.Count - 1; ndx >= 1; ndx--)
            {
                string possibleWord = _possibleWords[ndx];

                // Do not overtake functions if functions are explicity called with parenthesis "(" ")"
                //if (_parser.Context.Functions.Contains(possibleWord))
                //    break;

                if (words.ContainsKey(possibleWord))
                {
                    found = true;
                    _word = possibleWord;
                    _idCount = ndx;
                    break;
                }
            }
            return found;
        }


        /// <summary>
        /// @addwords( nasdaq, fluent script )
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var startToken = _tokenIt.NextToken;
            if(_idCount > 0)
                _tokenIt.Advance(_idCount);

            // Finally move past this plugin.
            _tokenIt.Advance();
            return Exprs.Const(new LString(_word), startToken);
        }
    }
}
