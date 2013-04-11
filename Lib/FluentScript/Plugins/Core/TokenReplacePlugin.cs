using System.Collections.Generic;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Core
{
    
    /// <summary>
    /// Plugin for replacing tokens.
    /// </summary>
    public class TokenReplacePlugin : TokenPlugin
    {
        private string _replacementToken;
        private bool _matched;
        private int _matchedAdvanceCount;


        /// <summary>
        /// The maximum number of tokens that can be looked ahead for potential replacements.
        /// </summary>
        protected int _maxLookAhead = 5;

        
        /// <summary>
        /// List of replacement word to their values.
        /// </summary>
        protected string[,] _replacements;


        /// <summary>
        /// A map of the replacement words to their value.
        /// </summary>
        protected Dictionary<string, string> _replaceMap;


        /// <summary>
        /// Replacement map for single words.
        /// </summary>
        protected Dictionary<string, string> _replaceMapSingleWord;


        private const string Partial_Replacement = "partial replacement";
        

        /// <summary>
        /// Initialize
        /// </summary>
        public TokenReplacePlugin()
        {
            _replaceMap = new Dictionary<string, string>();
            _replaceMapSingleWord = new Dictionary<string, string>();
        }


        /// <summary>
        /// Initialize multi-token replacements.
        /// </summary>
        /// <param name="replacements">The 2 dimension string array of text to their replacement values.</param>
        /// <param name="maxTokenLookAhead">The maximum tokens to look ahead for to seach for matching phrases.</param>
        public void Init(string[,] replacements, int maxTokenLookAhead)
        {
            _replacements = replacements;
            _maxLookAhead = maxTokenLookAhead;
            for (int ndx = 0; ndx < replacements.GetLength(0); ndx++)
            {
                var tokenToReplace = (string) replacements.GetValue(ndx, 0);
                var replaceVal = (string) replacements.GetValue(ndx, 1);
                this.SetupReplacement(tokenToReplace, replaceVal);
            }
        }


        public void SetupReplacement(string tokenToReplace, string replaceVal)
        {
            var hasSpace = tokenToReplace.Contains(" ");
            var mapHasToken = _replaceMap.ContainsKey(tokenToReplace);
                
            // Single word replacements "before" = "<"
            if (!hasSpace)
            {
                _replaceMapSingleWord[tokenToReplace] = replaceVal;
                return;
            }

            // Multi-word replacements. "less than" = "<" "less than equal to" = "<="
            string[] tokens = tokenToReplace.Split(' ');
            string multiTokenWord = tokens[0];
            _replaceMap[multiTokenWord] = Partial_Replacement;

            for(int ndx2 = 1; ndx2 < tokens.Length; ndx2++)
            {
                multiTokenWord += " " + tokens[ndx2];
                bool isLastToken = ndx2 == tokens.Length - 1;

                // 1. "less than" with "<"
                // 2. "less than equal" "<="
                bool replacementExists = _replaceMap.ContainsKey(multiTokenWord);
                if (!replacementExists && !isLastToken)
                {
                    _replaceMap[multiTokenWord] = Partial_Replacement;
                    _replaceMap[multiTokenWord + "END"] = replaceVal;
                }
                if ( isLastToken)
                    _replaceMap[multiTokenWord] = replaceVal;
            }
        }


        /// <summary>
        /// Whether or not this can handle the token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            return CanHandle(current, true);
        }


        /// <summary>
        /// Whether or not this can handle the token.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isCurrent">Whether or the token supplied is the current token. 
        /// if false.. it's assumed to be the next token( 1 token ahead ).</param>
        /// <returns></returns>
        public override bool CanHandle(Token token, bool isCurrent)
        {
            var t = token;
            _matchedAdvanceCount = isCurrent ? 1 : 2;            
            int advanceCount = isCurrent ? 1 : 2;
            string multiTokenWord = t.Text;
            _replacementToken = null;

            // Case 1: Check if single word replacement.
            if(IsSingleWordMatch(multiTokenWord))
            {
                 _matched = true;
                 _replacementToken = _replaceMapSingleWord[multiTokenWord];
                 
                // e.g. 
                // Code: book.author is 'maleev'                

                // Case 1: _tokenIt.NextToken = is    
                // token : is
                // result: Do not advance ( direct replacement )
               
                // Case 2: _tokenIt.NextToken = author
                // token : is ( ahead )
                // result: Advance only 1 token.
                 if (!isCurrent)
                     _matchedAdvanceCount = 1;   
                return true;
            }
                    
            // Case 2: Multi token replacement check:
            // Keep looking up to max look ahead times.
            while (advanceCount <= _maxLookAhead && !_tokenIt.IsEnded)
            {
                // 1. peek and combine next word
                string multiTokenWordPeek = multiTokenWord + " " + _tokenIt.Peek(advanceCount).Token.Text;

                if (_replaceMap.ContainsKey(multiTokenWord))
                {
                    var replacement = _replaceMap[multiTokenWord];
                    var isNextWordApplicable = _replaceMap.ContainsKey(multiTokenWordPeek);

                    // If next word is not applicable and current is "partial replacement"
                    if (!isNextWordApplicable && replacement == Partial_Replacement)
                    {
                        replacement = _replaceMap[multiTokenWord + "END"];
                    }
                    // Actual value to replace
                    if (!isNextWordApplicable)
                    {
                        _replacementToken = replacement;
                        _matched = true;
                        _matchedAdvanceCount = advanceCount;
                        break;
                    }                    
                }

                var tok = _tokenIt.Peek(advanceCount);
                t = tok.Token;
                advanceCount++;
                multiTokenWord += " " + t.Text;                                
            }
            return _matched;
        }


        
        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token Parse()
        {
            return Parse(false, 0);
        }


        /// <summary>
        /// Parse by optionally moving the token iterator forward first.
        /// </summary>
        /// <param name="advanceFirst"></param>
        /// <param name="advanceCount"></param>
        /// <returns></returns>
        public override Token Parse(bool advanceFirst, int advanceCount)
        {
            if (_matched)
            {
                if (advanceFirst)
                    _tokenIt.Advance(advanceCount);

                if (_matchedAdvanceCount > 1)
                    _tokenIt.Advance(_matchedAdvanceCount - 1);
            }
            var token = Tokens.AllTokens[_replacementToken];
            return token;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Token Peek()
        {
            var token = Tokens.AllTokens[_replacementToken];
            return token;
        }


        private bool IsSingleWordMatch(string multiTokenWord)
        {
            if (!_replaceMapSingleWord.ContainsKey(multiTokenWord)) return false;
            
            var token = _tokenIt.Peek();
            string combined = multiTokenWord + " " + token.Token.Text;

            // The next word combined with the current word could also be a token replacement.
            // e.g. is 3 
            // so if the combined word "is 3" is not in the replace map, then the current "is"
            // is a single word replacement.
            if (!_replaceMap.ContainsKey(combined))
                return true;

            return false;
        }
    }
}
