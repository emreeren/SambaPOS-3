using System;
using System.Collections.Generic;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser
{
    /// <summary>
    /// Iterates over a series of tokens in a script using a lexer.
    /// </summary>
    public class TokenIterator
    {
        private int _lastLineNumber;
        private int _lastCharPosition;
        private bool _isEnded;
        private Stack<int> _marks;
        private bool _isLLKEnabled = false;
        private int _LLK = -1;
        private int _tokenBatchMidPoint = -1;
        private int _tokenBatchMidPointIndex = -1;
        private Func<int, List<TokenData>> _tokensFetcher;
        private Action<int> _resetPosExecutor;
        private List<TokenData> _rewindBatch;
        

        /// <summary>
        /// The index position of the token in the current batch of tokens.
        /// </summary>
        protected int _currentBatchIndex;


        /// <summary>
        /// The index position of the token in the total tokens processed so far.
        /// </summary>
        protected int _currentIndex;


        /// <summary>
        /// The parsed tokens from the script
        /// </summary>
        public List<TokenData> TokenList;
        

        /// <summary>
        /// Last token parsed.
        /// </summary>
        public TokenData LastToken;


        /// <summary>
        /// The next token
        /// </summary>
        public TokenData NextToken;


        /// <summary>
        /// The path to the script.
        /// </summary>
        public string ScriptPath;



        /// <summary>
        /// The index position of the currrent token being processed
        /// </summary>
        public int CurrentIndex { get { return _currentIndex; } set { _currentIndex = value; } }


        /// <summary>
        /// The index position of the currrent token in the current batch of tokens.
        /// </summary>
        public int CurrentBatchIndex { get { return _currentBatchIndex; } set { _currentBatchIndex = value; } }


        /// <summary>
        /// The setting representing the number of tokens / batch to have at any one time.
        /// </summary>
        public int LLK { get { return _LLK; } set { _LLK = value; } }


        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="tokenFetcher">The callback used to get a batch of tokens.</param>        
        /// <param name="llK">The batch size of the tokens.</param>
        /// <param name="resetPosExecutor">The callback to use to reset the position when rewinding a token.</param>
        public void Init(Func<int, List<TokenData>> tokenFetcher, int llK, Action<int> resetPosExecutor)
        {
            _tokensFetcher = tokenFetcher;
            _resetPosExecutor = resetPosExecutor;
            _LLK = llK;

            // Validate inputs
            if (llK < 4) 
                throw new ArgumentException("Can not initialize token iterator with llk less than 4");

            int batchSize = 2 * llK;
            // NOTES
            // 1. llk has a slightly different meaning in this lexer than the typical LL(k) terminlogy in language design.
            // 2. llk represents the batch size of the tokens to retrieve from the lexer
            // 3. llk (batch size) of 8 indicates that this will request 8 tokens at a time from the lexer
            // 4. llk is used get tokens in batches as opposed to getting all the tokens at once from the lexer
            // 5. llk is used to reduce memory footprint for storing tokens. At any one point there is a list of llk count tokens available
            // 6. if llk is not set ( eg. -1 ) then the number of tokens retrieved from the lexer equal the number of tokens in a script
            // 7. the number of tokens in a script can go into the thousands ( larger memory requirement )
            // 8. An LL(4) indicates a look ahead level of minimum 3, maximum 6

            // MID POINT:
            // e.g. if 
            // 1. llk = 4, batchsize = 8, midpoint = 4
            // 2. llk = 5, batchsize = 10, midpoing = 5
            _tokenBatchMidPoint = batchSize / 2;
            _tokenBatchMidPointIndex = _tokenBatchMidPoint - 1;
            // NOTES: mid is the last position of the current index in the batchsize before it requests 
            // the next batch of LLK count tokens from the lexer
            _isLLKEnabled = true;
            CurrentIndex = -1;
            CurrentBatchIndex = -1;
            _lastCharPosition = 0;
            _lastLineNumber = 0;
            _isEnded = false;
            _marks = new Stack<int>();
            TokenList = _tokensFetcher(batchSize);
        }


        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="tokens">The list of tokens</param>
        /// <param name="lastLineNumber">Last line number of script</param>
        /// <param name="lastCharPos">Last char position of script.</param>
        public void Init(List<TokenData> tokens, int lastLineNumber, int lastCharPos)
        {
            TokenList = tokens;
            CurrentIndex = -1;
            CurrentBatchIndex = -1;
            _lastCharPosition = lastCharPos;
            _lastLineNumber = lastLineNumber;
            _marks = new Stack<int>();
        }


        /// <summary>
        /// Indicates whether or not the token iteration is ended.
        /// </summary>
        /// <returns></returns>
        public bool IsEnded
        {
            get { return _isEnded; }
        }


        /// <summary>
        /// Whether the current token is an end of statement token.
        /// </summary>
        /// <returns></returns>
        public bool IsExplicitEndOfStmt()
        {
            var t = NextToken.Token;
            if (t == Tokens.Semicolon || t == Tokens.NewLine || t == Tokens.EndToken)
                return true;
            return false;
        }


        /// <summary>
        /// Whether the current token is an end of statement token.
        /// </summary>
        /// <returns></returns>
        public bool IsEndOfStmtOrBlock()
        {
            var t = NextToken.Token;
            if (t == Tokens.Semicolon || t == Tokens.NewLine || t == Tokens.EndToken || t == Tokens.RightBrace)
                return true;
            return false;
        }


        /// <summary>
        /// Peek into and get the token ahead of the current token.
        /// </summary>
        /// <param name="count">The number of tokens to peek forward at</param>
        /// <param name="passNewLine">Whether or not the peek past the newline.</param>
        /// <returns></returns>
        public TokenData Peek(int count = 1, bool passNewLine = true)
        {
            if (CurrentIndex >= TokenList.Count - 1)
                return new TokenData() { Token = Tokens.EndToken, Line = _lastLineNumber, LineCharPos = _lastCharPosition };

            if (_isLLKEnabled && count > _LLK)
                throw new ArgumentException("Can not peek past llk of : " + _LLK);

            int ndx = CurrentIndex + 1;
            TokenData next = null;
            int advanced = 0;
            if (ndx >= TokenList.Count)
                throw new ArgumentException("Peeking past llk");

            while (ndx <= TokenList.Count - 1)
            {
                next = TokenList[ndx];

                // Is New line important ?
                if (!passNewLine && next.Token == Tokens.NewLine)
                    advanced++;

                else if (next.Token != Tokens.WhiteSpace 
                    && next.Token != Tokens.CommentMLine
                    && next.Token != Tokens.CommentSLine 
                    && next.Token != Tokens.NewLine)
                    advanced++;

                if (advanced == count) break;
                ndx++;
            }
            return next;
        }


        #region Advance calls
        /// <summary>
        /// Advances to the next token and returns the next token.
        /// </summary>
        public TokenData Advance(int count = 1, bool passNewLine = false)
        {            
            int advanced = 0;

            while (true)
            {
                LastToken = NextToken;

                // 1. Get the next batch ( LLK ) of tokens.
                if (_isLLKEnabled && _currentIndex == _tokenBatchMidPointIndex)
                {
                    if (NextToken.Token == Tokens.EndToken)
                    {
                        _isEnded = true;
                        break;
                    }

                    var tokens = _tokensFetcher(_LLK);
                    _rewindBatch = TokenList.GetRange(0, _tokenBatchMidPoint);
                    TokenList.RemoveRange(0, _tokenBatchMidPoint);
                    TokenList.AddRange(tokens);
                    CurrentIndex = -1;
                    CurrentBatchIndex = -1;
                }

                // Gaurd against empty or going past the last token
                // Scenario: At the end of the token list
                if (!_isLLKEnabled && TokenList.Count == 0 || CurrentIndex + 1 >= TokenList.Count)
                {
                    _isEnded = true;
                    return null;
                }

                CurrentIndex++;
                NextToken = TokenList[CurrentIndex];

                // Is New line important ?
                if (!passNewLine && NextToken.Token == Tokens.NewLine)
                    advanced++;

                else if (NextToken.Token != Tokens.WhiteSpace
                       && NextToken.Token != Tokens.CommentMLine
                       && NextToken.Token != Tokens.NewLine
                   )
                {
                    advanced++;
                }
                if (advanced == count)
                    break;
            }
            _currentBatchIndex = _currentIndex;
            return NextToken;
        }


        /// <summary>
        /// Advance to the next token and expect the token supplied.
        /// </summary>
        /// <param name="expectedToken">The token to expect after first advancing.</param>
        public void AdvanceAndExpect(Token expectedToken)
        {
            Advance();
            Expect(expectedToken);
        }


        /// <summary>
        /// Advance and get the next token.
        /// </summary>
        /// <typeparam name="T">The type of the token to get.</typeparam>
        /// <returns></returns>
        public T AdvanceAndGet<T>() where T : class
        {
            Advance();
            if (!(NextToken.Token is T)) throw BuildSyntaxExpectedException(typeof(T).Name.Replace("Token", ""));
            if (NextToken.Token == Tokens.EndToken) throw BuildEndOfScriptException();

            T token = NextToken.Token as T;
            return token;
        }


        /// <summary>
        /// Advances past newlines.
        /// </summary>
        public void AdvancePastNewLines()
        {
            if (NextToken.Token == Tokens.EndToken)
                return;

            if (NextToken.Token != Tokens.NewLine)
                return;

            while(NextToken.Token == Tokens.NewLine && !IsEnded)
            {
                Advance();    
            }
        }
        #endregion


        #region Expect calls
        /// <summary>
        /// Expect the end of statement.
        /// </summary>
        public void ExpectEndOfStmt()
        {
            // 1. End of statement. ; <newline> <eof>(end of script)
            if (IsExplicitEndOfStmt())
            {
                Advance();
                return;
            }
            // 2. end block e.g. } - let block code expect "}"
            if (NextToken.Token == Tokens.RightBrace)
                return;

            // 3. Something else ?
            throw BuildSyntaxExpectedException("End of statement");            
        }


        /// <summary>
        /// Expect the token supplied and advance to next token
        /// </summary>
        /// <param name="token"></param>
        public void Expect(Token token)
        {
            if (NextToken.Token == Tokens.EndToken) throw BuildEndOfScriptException();
            if (NextToken.Token != token) throw BuildSyntaxExpectedException(token.Text);
            Advance();
        }


        /// <summary>
        /// Expect the token supplied and advance to next token
        /// </summary>
        /// <param name="token1">The 1st token to expect</param>
        /// <param name="token2">The 2nd token to expect</param>
        /// <param name="token3">The 3rd token to expect</param>
        public void ExpectMany(Token token1, Token token2 = null, Token token3 = null)
        {
            if (NextToken.Token == Tokens.EndToken) throw BuildEndOfScriptException();

            // ASSERT token 1 match
            if (NextToken.Token != token1) throw BuildSyntaxExpectedException(token1.Text);

            // No token 2 ?
            if (token2 == null) { Advance(); return; }

            // Advance to token 2
            Advance();

            // ASSERT token 2 match
            if (NextToken.Token != token2) throw BuildSyntaxExpectedException(token2.Text);

            // No token 3
            if (token3 == null) { Advance(); return; }

            // Advance to token 3.
            Advance();

            // ASSERT token 3 match
            if (NextToken.Token != token3) throw BuildSyntaxExpectedException(token3.Text);

            Advance();
        }


        /// <summary>
        /// Expect identifier
        /// </summary>
        /// <param name="advance">Whether or not to advance to next token</param>
        /// <param name="allowLiteralAsId">Whether or not to allow a literal as an identifier.</param>
        /// <returns></returns>
        public string ExpectId(bool advance = true, bool allowLiteralAsId = false)
        {
            if (NextToken.Token == Tokens.EndToken) throw BuildEndOfScriptException();
            if (!allowLiteralAsId && !(NextToken.Token.Kind == TokenKind.Ident)) throw BuildSyntaxExpectedException("identifier");

            string id = NextToken.Token.Text;

            if (advance)
                Advance();

            return id;
        }


        /// <summary>
        /// Expect identifier
        /// </summary>
        /// <param name="text">The text of the id token to expect</param>
        /// <param name="advance">Whether or not to advance to next token</param>
        /// <returns></returns>
        public string ExpectIdText(string text, bool advance = true)
        {
            if (NextToken.Token == Tokens.EndToken) throw BuildEndOfScriptException();
            if (!(NextToken.Token.Kind == TokenKind.Ident)) throw BuildSyntaxExpectedException("identifier");

            string id = NextToken.Token.Text;
            if (id != text) throw BuildSyntaxExpectedException(text);

            if (advance)
                Advance();

            return id;
        }


        /// <summary>
        /// Expect identifier
        /// </summary>
        /// <param name="advance">Whether or not to advance to next token</param>
        /// <returns></returns>
        public double ExpectNumber(bool advance = true)
        {
            if (NextToken.Token == Tokens.EndToken) throw BuildEndOfScriptException();
            if (!(NextToken.Token.IsLiteralAny())) throw BuildSyntaxExpectedException("number");

            string val = NextToken.Token.Text;
            double num = 0;
            if (!double.TryParse(val, out num)) throw BuildSyntaxExpectedException("number");

            if (advance)
                Advance();

            return num;
        }


        /// <summary>
        /// Expect a string literal
        /// </summary>
        /// <param name="expectedText">The text to expect, this can be null if only a string token is expected without any specific text.</param>
        /// <param name="matchCase">Whether or not match the case of the string token if expectingText param is true.</param>
        /// <param name="advance">Whether or not to advance to next token</param>
        /// <returns></returns>
        public string ExpectString(string expectedText, bool matchCase = true, bool advance = true)
        {
            if (NextToken.Token == Tokens.EndToken) throw BuildEndOfScriptException();
            if (!(NextToken.Token.Type == TokenTypes.LiteralString)) throw BuildSyntaxExpectedException("string");

            string tokenText = NextToken.Token.Text;

            // Compare expected text if supplied.
            if (!string.IsNullOrEmpty(expectedText))
            {
                StringComparison c = matchCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                if (string.Compare(expectedText, tokenText, c) != 0)
                {
                    throw BuildSyntaxExpectedException(expectedText);
                }
            }
            if (advance)
                Advance();

            return tokenText;
        }
        #endregion


        #region Helpers
        /// <summary>
        /// Gets a list of consequtive id tokens appended to form potential names.
        /// e.g. [ "refill", "refill inventory" ]
        /// </summary>
        public List<string> PeekConsequetiveIdsAppended(int maxTokenLookAhead)
        {
            int total = 1;
            var currentWord = NextToken.Token.Text;
            var ahead = Peek(1, false);

            var possibleWords = new List<string>();
            possibleWords.Add(currentWord);
            var combinedWord = currentWord;

            // Build up the word until , is hit.
            while (ahead.Token.Kind == TokenKind.Ident && total < maxTokenLookAhead)
            {
                total++;                
                combinedWord += " " + ahead.Token.Text;
                possibleWords.Add(combinedWord);                
                ahead = Peek(total, false);
            }
            return possibleWords;
        }


        /// <summary>
        /// Gets a list of consequtive id tokens appended to form potential names.
        /// e.g. [ "refill", "refill inventory" ]
        /// </summary>
        public List<Tuple<string, int>> PeekConsequetiveIdsAppendedWithTokenCounts(bool enableCamelCasingAsSeparateWords, int maxTokenLookAhead)
        {
            int total = 1;
            var currentWord = NextToken.Token.Text;
            var ahead = Peek(1, false);

            var possibleWords = new List<Tuple<string, int>>();
            possibleWords.Add(new Tuple<string, int>(currentWord, total));
            var combinedWord = currentWord;
            var camelCasedWord = currentWord;

            // Build up the word until , is hit.
            while (ahead.Token.Kind == TokenKind.Ident && total < maxTokenLookAhead)
            {
                total++;
                combinedWord += " " + ahead.Token.Text;
                possibleWords.Add(new Tuple<string, int>(combinedWord, total));
                if (enableCamelCasingAsSeparateWords)
                {
                    var tokenText = ahead.Token.Text;
                    string upperWord = Char.ToUpper(tokenText[0]).ToString();
                    if (tokenText.Length > 1)
                        upperWord += tokenText.Substring(1);
                    camelCasedWord += upperWord;
                    possibleWords.Add(new Tuple<string, int>(camelCasedWord, total));
                }
                ahead = Peek(total, false);
            }
            return possibleWords;
        }


        /// <summary>
        /// Gets a list of consequtive id tokens
        /// returns list of idtokens.
        /// e.g. [ refill, inventory ]
        /// </summary>
        public List<Token> PeekConsequetiveIdTokens()
        {
            int total = 1;
            var currentWord = NextToken.Token;
            var ahead = Peek(1, false);

            var ids = new List<Token>();
            ids.Add(currentWord);

            // Build up the word until , is hit.
            while (ahead.Token.Kind == TokenKind.Ident)
            {
                total++;
                ids.Add(ahead.Token);
                ahead = Peek(total, false);
            }
            return ids;
        }


        /// <summary>
        /// Gets a list of consequtive id tokens
        /// returns list of idtokens.
        /// e.g. [ refill, inventory ]
        /// </summary>
        public List<TokenData> PeekConsequetiveTokens(int count)
        {
            var tokens = new List<TokenData>();
            tokens.Add(NextToken);
            for (int c = 1; c < count; c++)
            {
                var token = Peek(c);
                tokens.Add(token);
            }
            return tokens;
        }
        #endregion


        #region Parser Errors
        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public LangException BuildSyntaxException(string errorMessage)
        {
            return new LangException("Syntax Error", errorMessage, ScriptPath, NextToken.Line, NextToken.LineCharPos);
        }


        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="token">The token associated with the error.</param>
        /// <returns></returns>
        public LangException BuildSyntaxException(string errorMessage, TokenData token)
        {
            return new LangException("Syntax Error", errorMessage, ScriptPath, token.Line, token.LineCharPos);
        }


        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="node">The AST node</param>
        /// <returns></returns>
        public LangException BuildSyntaxException(string errorMessage, AstNode node)
        {
            return new LangException("Syntax Error", errorMessage, ScriptPath, node.Ref.Line, node.Ref.CharPos);
        }
        
        
        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        public LangException BuildSyntaxExpectedException(string expected)
        {
            return new LangException("Syntax Error", string.Format("Expected {0} but found '{1}'", expected, NextToken.Token.Text), ScriptPath, NextToken.Line, NextToken.LineCharPos);
        }


        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <param name="token">The expected token</param>
        /// <returns></returns>
        public LangException BuildSyntaxExpectedTokenException(Token token)
        {
            return new LangException("Syntax Error", string.Format("Expected {0} but found '{1}'", token.Text, NextToken.Token.Text), ScriptPath, NextToken.Line, NextToken.LineCharPos);
        }


        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <returns></returns>
        public LangException BuildSyntaxUnexpectedTokenException()
        {
            return new LangException("Syntax Error", string.Format("Unexpected token found '{0}'", NextToken.Token.Text), ScriptPath, NextToken.Line, NextToken.LineCharPos);
        }


        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <returns></returns>
        public LangException BuildSyntaxUnexpectedTokenException(TokenData token)
        {
            return new LangException("Syntax Error", string.Format("Unexpected token found '{0}'", token.Token.Text), ScriptPath, token.Line, token.LineCharPos);
        }


        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <param name="unexpectedTokenText">The text of the token</param>
        /// <param name="token">The token causing the exception</param>
        /// <returns></returns>
        public LangException BuildSyntaxUnexpectedTokenException(string unexpectedTokenText, TokenData token)
        {
            return new LangException("Syntax Error", string.Format("Unexpected token found {0}", unexpectedTokenText), ScriptPath, token.Line, token.LineCharPos);
        }


        /// <summary>
        /// Builds a language exception due to the unexpected end of script.
        /// </summary>
        /// <returns></returns>
        public LangException BuildEndOfScriptException()
        {
            return new LangException("Syntax Error", "Unexpected end of script", ScriptPath, NextToken.Line, NextToken.LineCharPos);
        }


        /// <summary>
        /// Builds a language exception due to a specific limit being reached.
        /// </summary>
        /// <param name="error">Error message</param>
        /// <param name="limittype">FuncParameters</param>
        /// <param name="limit">Limit number</param>
        /// <returns></returns>
        public LangException BuildLimitException(string error, int limit, string limittype = "")
        {
            return new LangException("Limit Error", error, ScriptPath, NextToken.Line, NextToken.LineCharPos);
        }
        #endregion
    }
}
