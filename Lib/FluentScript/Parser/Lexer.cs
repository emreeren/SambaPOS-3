using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ComLib.Lang
{
    /// <summary>
    /// Converts script from a series of characters into a series of tokens.
    /// Main method is NextToken();
    /// A script can be broken down into a sequence of tokens.
    /// e.g.
    /// 
    /// 1. var name = "kishore";
    /// Tokens:
    /// 
    ///  TOKEN VALUE:         TOKEN TYPE:
    ///  var         keyword
    ///  ""          literal ( whitespace )
    ///  name        id
    ///  ""          literal ( whitespace )
    ///  =           operator
    ///  ""          literal ( whitespace )
    ///  "kishore"   literal
    ///  ;           operator
    /// </summary>
    public class Lexer : LexerBase
    {
        enum TokenLengthCalcMode
        {
            /// <summary>
            /// Use the size the token text
            /// </summary>
            Direct,


            /// <summary>
            /// Use the length of whitespace
            /// </summary>
            WhiteSpace,


            /// <summary>
            /// Use the length of the token plus 2 chars for string literal
            /// </summary>
            String,


            /// <summary>
            /// MUltiline comment.
            /// </summary>
            MultilineComment
        }


        #region Private members
        private Context _ctx;      
        private Token _lastToken;
        private TokenData _lastTokenData;
        private int _tokenIndex = -1;
        private bool _hasReplacementsOrRemovals = false;
        private char _interpolatedStartChar = '#';
        
        private List<TokenData> _tokens;
        private Dictionary<string, string> _replacements = new Dictionary<string, string>();
        private Dictionary<string, Tuple<bool, string>> _inserts = new Dictionary<string, Tuple<bool, string>>();
        private Dictionary<string, bool> _removals = new Dictionary<string, bool>();
        private static IDictionary<char, bool> _opChars = new Dictionary<char, bool>()
        {
            { '*', true},
            { '/', true},
            { '+', true},
            { '-', true},
            { '%', true},
            { '<', true},
            { '>', true},
            { '=', true},
            { '!', true},
            { '&', true},
            { '|', true}             
        };
        #endregion


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="text">The text to parse</param>
        public Lexer(string text) : this(new Context(), text)
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="ctx">The context containing plugins among other core integration components.</param>
        /// <param name="text">The source code text to lex</param>
        public Lexer(Context ctx, string text)
        {
            _ctx = ctx;
            Init(text, '\\', new char[] { SQUOTE, DQUOTE }, new char[] { ' ', '\t' });
        }


        /// <summary>
        /// Initialize with the text.
        /// </summary>
        /// <param name="text"></param>
        public void Init(string text)
        {
            Init(text, '\\', new char[] { SQUOTE, DQUOTE }, new char[] { ' ', '\t' });
        }        

        
        /// <summary>
        /// Starting char which signifies the start of an expression in an interpolated string.
        /// </summary>
        public char IntepolatedStartChar { get { return _interpolatedStartChar; } set { _interpolatedStartChar = value; } }

       
        /// <summary>
        /// Replaces a token with another token.
        /// </summary>
        /// <param name="text">The text to replace</param>
        /// <param name="newValue">The replacement text</param>
        public void SetReplacement(string text, string newValue)
        {
            // check if replacements has a space. can do at most 2 words in a replacement for now.
            // e.g. can replace "number of" with "count".
            _replacements[text] = newValue;
            _hasReplacementsOrRemovals = true;
        }


        /// <summary>
        /// Removes a token during the lexing process.
        /// </summary>        
        /// <param name="text">The text to remove</param>
        public void SetRemoval(string text)
        {
            _removals[text] = true;
            _hasReplacementsOrRemovals = true;
        }


        /// <summary>
        /// Adds a token during the lexing process.
        /// </summary>
        /// <param name="before">whether to insert before or after</param>
        /// <param name="text">The text to check for inserting before/after</param>
        /// <param name="newValue">The new value to insert before/after</param>
        public void SetInsert(bool before, string text, string newValue)
        {
            _inserts[text] = new Tuple<bool, string>(before, newValue);
        }


        /// <summary>
        /// The current token.
        /// </summary>
        public Token LastToken { get { return _lastToken; } }


        /// <summary>
        /// The current token.
        /// </summary>
        public TokenData LastTokenData { get { return _lastTokenData; } }


        /// <summary>
        /// The list of parsed tokens.
        /// </summary>
        public List<TokenData> ParsedTokens { get { return _tokens; } }


        /// <summary>
        /// Returns a list of tokens of the entire script.
        /// </summary>
        /// <returns></returns>
        public List<TokenData> Tokenize()
        {
            return GetTokenBatch(-1);
        }


        /// <summary>
        /// Get the next batch of tokens.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TokenData> GetTokenBatch(int count)
        {
            _tokenIndex = -1;
            _tokens = new List<TokenData>();
            var hasPlugins = _ctx.Plugins.TotalLexical > 0;

            TokenData last = null;
            while (true)
            {               
                var token = NextToken();

                // Set the index position of the token in the script.
                if (token.Token != Tokens.WhiteSpace)
                {
                    _tokenIndex++;
                    token.SetIndex(_tokenIndex);
                }

                // 1. End of script ?
                if (token.Token == Tokens.EndToken)
                {
                    _tokens.Add(token);
                    break;
                }

                // 2. Null token ?
                if (token.Token == null) 
                    continue;

                // Avoid storing white space tokens.
                if (token.Token != Tokens.WhiteSpace)
                {
                    // 3. Plugins? 
                    if (hasPlugins && _ctx.Plugins.CanHandleLex(token.Token))
                    {
                        var plugin = _ctx.Plugins.LastMatchedLexPlugin;
                        plugin.Parse();
                    }

                    // 4. Can immediately add to tokens ?
                    else if (!_hasReplacementsOrRemovals)
                        _tokens.Add(token);

                    // 5. Special Handling Cases
                    //    Case 1: Replace token ?
                    else if (_replacements.ContainsKey(token.Token.Text))
                    {
                        var replaceVal = _replacements[token.Token.Text];

                        // Replaces system token?
                        if (Tokens.AllTokens.ContainsKey(replaceVal))
                        {
                            Token t = Tokens.AllTokens[replaceVal];
                            token.Token = t;
                        }
                        else
                        {
                            token.Token.SetText(replaceVal);
                        }
                        _tokens.Add(token);
                    }
                    // Case 2: Remove token ?
                    else if (!_removals.ContainsKey(token.Token.Text))
                        _tokens.Add(token);
                }

                // If only getting limited number of tokens then get
                // the specified count number of tokens.
                if (count != -1 && _tokens.Count >= count)
                {
                    break;
                }

                //DEBUG.ASSERT. Did not progress somehow.
                if(last == token)
                    throw new LangException("Syntax Error", "Unexpected token", string.Empty, _pos.Line, _pos.LineCharPosition);
                last = token;
            }
            return _tokens;
        }

        
        /// <summary>
        /// Reads the next token from the reader.
        /// </summary>
        /// <returns> A token, or <c>null</c> if there are no more tokens. </returns>
        public TokenData NextToken()
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // LEXER ALWAYS READS NEXT CHAR
            char c = _pos.CurrentChar;
            char n = PeekChar();
            var tokenLengthCalcMode = TokenLengthCalcMode.Direct;

            int pos = _pos.Pos;
            int line = _pos.Line;
            int tokenLength = 0;
            int cpos = _pos.LineCharPosition;
            
            if (IsEnded())
            {
                _lastToken = Tokens.EndToken;
            }
            // Empty space.
            else if (c == ' ' || c == '\t')
            {
                ConsumeWhiteSpace(false, true);
                _lastToken = Tokens.WhiteSpace;
                tokenLength = (_pos.Pos - pos) + 1;
                tokenLengthCalcMode = TokenLengthCalcMode.WhiteSpace;
            }
            // Variable
            else if (IsIdStartChar(c))
            {
                _lastToken = ReadWord();
            }
            // Single line
            else if (c == '/' && n == '/')
            {
                MoveChars(2);
                var result = ScanToNewLine(false, true);
                tokenLengthCalcMode = TokenLengthCalcMode.String;
                tokenLength = (_pos.Pos - pos) + 1;
                _lastToken = Tokens.ToComment(false, result.Text);
            }
            // Multi-line
            else if (c == '/' && n == '*')
            {
                MoveChars(2);
                var result = ScanUntilChars(false, '*', '/', true);
                tokenLengthCalcMode = TokenLengthCalcMode.MultilineComment;
                tokenLength = _pos.LineCharPosition;
                _lastToken = Tokens.ToComment(true, result.Text);
            }
            else if (c == '|' && n != '|')
            {
                _lastToken = Tokens.Pipe;
            }
            // Operator ( Math, Compare, Increment ) * / + -, < < > >= ! =
            else if (IsOp(c) == true)
            {
                _lastToken = ReadOperator();
            }
            else if (c == '(')
            {
                _lastToken = Tokens.LeftParenthesis;
            }
            else if (c == ')')
            {
                _lastToken = Tokens.RightParenthesis;
            }
            else if (c == '[')
            {
                _lastToken = Tokens.LeftBracket;
            }
            else if (c == ']')
            {
                _lastToken = Tokens.RightBracket;
            }
            else if (c == '.')
            {
                _lastToken = Tokens.Dot;
            }
            else if (c == ',')
            {
                _lastToken = Tokens.Comma;
            }
            else if (c == ':')
            {
                _lastToken = Tokens.Colon;
            }
            else if (c == '{')
            {
                _lastToken = Tokens.LeftBrace;
            }
            else if (c == '}')
            {
                _lastToken = Tokens.RightBrace;
            }
            else if (c == ';')
            {
                _lastToken = Tokens.Semicolon;
            }
            else if (c == '$')
            {
                _lastToken = Tokens.Dollar;
            }
            else if (c == '@')
            {
                _lastToken = Tokens.At;
            }
            else if (c == '#')
            {
                _lastToken = Tokens.Pound;
            }
            else if (c == '\\')
            {
                _lastToken = Tokens.BackSlash;
            }
            // String literal
            else if (c == '"' || c == '\'')
            {
                _lastToken = ReadString( c == '"');
                tokenLengthCalcMode = TokenLengthCalcMode.String;
                if (_lastToken.Kind == TokenKind.Multi)
                {
                    tokenLength = (_pos.Pos - pos) -2;
                    string text = _pos.Text.Substring(pos + 1, tokenLength);
                    _lastToken.SetText(text);
                }
                else
                {
                    tokenLength = _lastToken.Text.Length + 2;
                }
            }
            else if (IsNumeric(c))
            {
                _lastToken = ReadNumber();
            }
            else if (c == '\r')
            {
                bool is2CharNewline = n == '\n';
                IncrementLine(is2CharNewline);
            }
            else
            {
                _lastToken = Tokens.Unknown;
            }
            var t = new TokenData() { Token = _lastToken, Line = line, LineCharPos = cpos, Pos = pos };
            _lastTokenData = t;

            // Single char symbol - char advancement was not made.
            if ((t.Token.Kind == TokenKind.Symbol || t.Token.Type == TokenTypes.Unknown || t.Token.Type == TokenTypes.WhiteSpace) && _pos.Pos == pos)
                ReadChar();

            // Before returning, set the next line char position.
            //if (_pos.LineCharPosition != 0 && _lastToken != null)
            //{
            //    if (_pos.LineCharPosition > 0)
            //    {
            //        var lineCharPos = 0;
            //        if (tokenLengthCalcMode == TokenLengthCalcMode.Direct)
            //            lineCharPos = _pos.LineCharPosition - 1 + _lastToken.Text.Length;

            //        else if (tokenLengthCalcMode == TokenLengthCalcMode.MultilineComment)
            //            lineCharPos = tokenLength;
            //        else
            //            lineCharPos = _pos.LineCharPosition - 1 + tokenLength;
            //        _pos.LineCharPosition = lineCharPos;
            //    }
            //}
            return t;
        }


        #region Peek methods
        /// <summary>
        /// Peeks at the next token.
        /// </summary>
        /// <returns></returns>
        public TokenData PeekToken(bool allowSpace = false)
        {
            var line = _pos.Line;
            var linepos = _pos.LineCharPosition;
            var lastToken = _lastToken;
            var lastTokenData = _lastTokenData;
            var iSc = _interpolatedStartChar;
            var pos = _pos.Pos;

            // Get the next token.
            var token = NextToken();
            if (!allowSpace && token.Token == Tokens.WhiteSpace)
            {
                while (token.Token == Tokens.WhiteSpace)
                {
                    token = NextToken();
                }
            }
            // Reset the data back to the last token.
            _pos.Line = line;
            _pos.LineCharPosition = linepos;
            _lastToken = lastToken;
            _lastTokenData = lastTokenData;
            _interpolatedStartChar = iSc;
            SetPosition(pos);
            return token;
        }

        
        /// <summary>
        /// Peeks at the next word that does not include a space.
        /// </summary>
        /// <returns></returns>
        public string PeekWord()
        {
            string text = _pos.Text;
            int currentPos = _pos.Pos + 1;

            string word = "";
            while (true && currentPos <= LAST_POSITION)
            {
                char c = text[currentPos];
                if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') ||
                     ('0' <= c && c <= '9') || (c == '_' || c == '-'))
                {
                    word += c;
                }
                else
                    break;

                currentPos++;
            }
            return word;
        }


        /// <summary>
        /// Peeks at the next word that does not include a space.
        /// </summary>
        /// <returns></returns>
        public string PeekPostiveNumber()
        {
            string text = _pos.Text;
            int currentPos = _pos.Pos + 1;

            string word = "";

            // TODO: Does not properly handle checking against multiple "."
            while (true && currentPos <= LAST_POSITION)
            {
                char c = text[currentPos];
                if (('0' <= c && c <= '9') || (c == '.'))
                {                    
                    word += c;
                }
                else
                    break;

                currentPos++;
            }
            return word;
        }


        /// <summary>
        /// Peeks at the next word that does not include a space or new line
        /// </summary>
        /// <param name="extra1">An extra character that is allowed to be part of the word in addition to the allowed chars</param>
        /// <param name="extra2">A second extra character that is allowed to be part of word in addition to the allowed chars</param>
        /// <param name="startPos">The starting positing to peek word from</param>
        /// <returns></returns>
        public KeyValuePair<int, string> PeekWord(int startPos = -1, char extra1 = char.MinValue, char extra2 = char.MinValue)
        {
            string text = _pos.Text;
            int currentPos = startPos == -1
                           ? _pos.Pos + 1
                           : _pos.Pos + startPos;

            string word = "";
            bool hasExtra1 = extra1 != char.MinValue;
            bool hasExtra2 = extra2 != char.MinValue;

            while (true && currentPos <= LAST_POSITION)
            {
                char c = text[currentPos];

                if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') ||
                     ('0' <= c && c <= '9') || (c == '_' || c == '-'))
                {
                    word += c;
                }
                else if ((hasExtra1 && c == extra1) || (hasExtra2 && c == extra2))
                    word += c;
                else
                    break;

                currentPos++;
            }
            return new KeyValuePair<int, string>(currentPos, word);
        }


        /// <summary>
        /// Peeks at the next word that does not include a space or new line
        /// </summary>
        /// <param name="expectChar">A char to expect while peeking at the next word.</param>
        /// <param name="maxAdvancesBeforeExpected">The maximum number of advances that the expectChar should appear by</param>        
        /// <param name="extra1">An extra character that is allowed to be part of the word in addition to the allowed chars</param>
        /// <param name="extra2">A second extra character that is allowed to be part of word in addition to the allowed chars</param>
        /// <returns></returns>
        public KeyValuePair<bool, string> PeekWordWithChar(char expectChar, bool startAtNextChar, int maxAdvancesBeforeExpected, char extra1 = char.MinValue, char extra2 = char.MinValue)
        {
            string text = _pos.Text;
            int currentPos = startAtNextChar ? _pos.Pos + 1 : _pos.Pos;
            bool found = false;
            int totalCharsRead = 0;

            string word = "";
            bool hasExtra1 = extra1 != char.MinValue;
            bool hasExtra2 = extra2 != char.MinValue;
            
            while (true && currentPos <= LAST_POSITION)
            {
                char c = text[currentPos];
                totalCharsRead++;

                if (c == expectChar)
                    found = true;

                if (totalCharsRead >= maxAdvancesBeforeExpected && !found)
                    break;

                if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') ||
                     ('0' <= c && c <= '9') || (c == '_' || c == '-'))
                {
                    word += c;
                }
                else if ((hasExtra1 && c == extra1) || (hasExtra2 && c == extra2))
                    word += c;
                else
                    break;

                currentPos++;
            }
            return new KeyValuePair<bool, string>(found, word);
        }
        #endregion



        #region Token Read methods
        /// <summary>
        /// Read word
        /// </summary>
        /// <returns></returns>
        public Token ReadWord()
        {
            var result = ScanId(false, true);

            // true / false / null
            if (Tokens.IsLiteral(result.Text))
                return Tokens.Lookup(result.Text);

            // var / for / while
            if (Tokens.IsKeyword(result.Text))
                return Tokens.Lookup(result.Text);

            return Tokens.ToIdentifier(result.Text);
        }


        /// <summary>
        /// Reads a uri such as http, https, ftp, ftps, www.
        /// </summary>
        /// <returns></returns>
        public Token ReadUri()
        {
            var result = ScanUri(false, true);
            return Tokens.ToLiteralString(result.Text);
        }


        /// <summary>
        /// Reads the next word that does not include a space or new line
        /// </summary>
        /// <param name="extra1">An extra character that is allowed to be part of the word in addition to the allowed chars</param>
        /// <param name="extra2">A second extra character that is allowed to be part of word in addition to the allowed chars</param>
        /// <returns></returns>
        public Token ReadWordWithExtra(char extra1 = char.MinValue, char extra2 = char.MinValue)
        {
            var result = ScanWordUntilChars(false, true, extra1, extra2);
            return Tokens.ToLiteralString(result.Text);
        }


        /// <summary>
        /// Read number
        /// </summary>
        /// <returns></returns>
        public Token ReadNumber()
        {
            var result = ScanNumber(false, true);
            return Tokens.ToLiteralNumber(result.Text);
        }


        /// <summary>
        /// Read an operator
        /// </summary>
        /// <returns></returns>
        public Token ReadOperator()
        {
            var result = ScanChars(_opChars, false, true);
            return Tokens.Lookup(result.Text);
        }


        /// <summary>
        /// Reads a string either in quote or double quote format.
        /// </summary>
        /// <returns></returns>
        public Token ReadString(bool handleInterpolation = true)
        {
            char quote = _pos.CurrentChar;
                
            // 1. Starts with either ' or "
            // 2. Handles interpolation "homepage of ${user.name} is ${url}"
            if (!handleInterpolation)
            {
                var result = ScanCodeString(quote, setPosAfterToken: true);
                if(!result.Success)
                    throw new LangException("Syntax Error", "Unterminated string", string.Empty, _pos.Line, _pos.LineCharPosition);

                return Tokens.ToLiteralString(result.Text);
            }
            return ReadInterpolatedString(quote, false, true, true);
        }


        /// <summary>
        /// Reads up to the position supplied.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Token ReadToPosition(int pos)
        {
            var result = ScanToPosition(true, pos);
            return Tokens.ToLiteralString(result.Text);
        }


        /// <summary>
        /// Reads string upto end of line.
        /// </summary>
        /// <returns></returns>
        public Token ReadLine(bool includeNewLine)
        {
            return ReadInterpolatedString(Char.MinValue, true, includeNewLine, true);
        }


        /// <summary>
        /// Reads string upto end of line.
        /// </summary>
        /// <returns></returns>
        public Token ReadLineRaw(bool includeNewLine)
        {
            var result = ScanToNewLine(false, true);
            var token = Tokens.ToLiteralString(result.Text);
            return token;
        }        


        /// <summary>
        /// Reads an interpolated string in format "${variable} some text ${othervariable + 2}."
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="readLine">Whether or not to only read to the end of the line.</param>
        /// <param name="includeNewLine">Whether or not to include the new line in parsing.</param>
        /// <param name="setPositionAfterToken">Whether or not set the position of lexer after the token.</param>
        /// <returns></returns>
        public Token ReadInterpolatedString(char quote, bool readLine = false, bool includeNewLine = false, bool setPositionAfterToken = true)
        {
            var allTokens = new List<TokenData>();
            var interpolationCount = 0;
            // Only supporting following:
            // 1. id's abcd with "_"
            // 2. "."
            // 3. math ops ( + - / * %)
            // "name" 'name' "name\"s" 'name\'"
            var buffer = new StringBuilder();
            char curr = ReadChar();
            char next = PeekChar();
            bool matched = false;
            char escapeChar = '\\';
            Token token = null;
            while (_pos.Pos <= LAST_POSITION)
            {
                // End string " or '
                if (!readLine && curr == quote)
                {
                    matched = true;
                    MoveChars(1);
                    break;
                }
                // End of line.
                if (readLine && curr == '\r' )
                {
                    matched = true;
                    if (!includeNewLine) break;

                    bool is2CharNewLine = next == '\n';
                    IncrementLine(is2CharNewLine);
                    token = Tokens.NewLine;
                    if (is2CharNewLine)
                        buffer.Append("\r\n");
                    else
                        buffer.Append("\n"); 
                    break;
                }
                // Interpolation.
                else if (curr == _interpolatedStartChar && next == '{')
                {
                    // Keep track of interpolations and their start positions.
                    interpolationCount++;
                    int interpolatedStringStartPos = LineCharPos + 2;
                    int interpolatedStringLinePos = LineNumber;

                    // Add any existing text before the interpolation as a token.
                    if (buffer.Length > 0)
                    {
                        string text = buffer.ToString();
                        token = Tokens.ToLiteralString(text);
                        var t = new TokenData() { Token = token, LineCharPos = 0, Line = LineNumber };
                        allTokens.Add(t);
                        buffer.Clear();
                    }
                    MoveChars(1);
                    var tokens = ReadInterpolatedTokens();
                    token = Tokens.ToInterpolated(string.Empty, tokens);
                    var iTokenData = new TokenData() { Token = token, LineCharPos = interpolatedStringStartPos, Line = interpolatedStringLinePos };
                    allTokens.Add(iTokenData);
                }
                // Not an \ for escaping so just append.
                else if (curr != escapeChar)
                {
                    buffer.Append(curr);
                }
                // Escape \
                else if (curr == escapeChar)
                {
                    if (next == quote) buffer.Append(quote);
                    else if (next == '\\') buffer.Append("\\");
                    else if (next == 'r') buffer.Append('\r');
                    else if (next == 'n') buffer.Append('\n');
                    else if (next == 't') buffer.Append('\t');
                    MoveChars(1);
                }

                curr = ReadChar();
                next = PeekChar();
            }
            
            // Error: Unterminated string constant.
            if (!matched && !readLine && _pos.Pos >= LAST_POSITION)
            {
                throw new LangException("Syntax Error", "Unterminated string", string.Empty, _pos.Line, _pos.LineCharPosition);
            }

            // At this point the pos is already after token.
            // If matched and need to set at end of token, move back 1 char
            if (matched && !setPositionAfterToken) MoveChars(-1);
            if (interpolationCount == 0)
            {
                string text = buffer.ToString();
                return Tokens.ToLiteralString(text);
            }
            if (buffer.Length > 0)
            {
                string text = buffer.ToString();
                token = Tokens.ToLiteralString(text);
                allTokens.Add(new TokenData() { Token = token, LineCharPos = 0, Line = LineNumber });
            }
            return Tokens.ToInterpolated(string.Empty, allTokens);
        }
        #endregion



        #region Private methods
        /// <summary>
        /// Increments the line number
        /// </summary>
        /// <param name="is2CharNewLine"></param>
        protected override void IncrementLine(bool is2CharNewLine)
        {
            base.IncrementLine(is2CharNewLine);
            _lastToken = Tokens.NewLine;
        }


        private List<TokenData> ReadInterpolatedTokens()
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // LEXER ALWAYS READS NEXT CHAR
            char c = ReadChar();
            char n = PeekChar();
            List<TokenData> tokens = new List<TokenData>();

            while (c != '}' && !IsAtEnd())
            {
                int pos = _pos.Pos;
            
                // Variable
                if (IsIdStartChar(c))
                {
                    _lastToken = ReadWord();
                }
                // Empty space.
                else if (c == ' ' || c == '\t')
                {
                    _lastToken = Tokens.WhiteSpace;
                }
                else if (IsOp(c) == true)
                {
                    _lastToken = ReadOperator();
                }
                else if (c == '(')
                {
                    _lastToken = Tokens.LeftParenthesis;
                }
                else if (c == ')')
                {
                    _lastToken = Tokens.RightParenthesis;
                }
                else if (c == '[')
                {
                    _lastToken = Tokens.LeftBracket;
                }
                else if (c == ']')
                {
                    _lastToken = Tokens.RightBracket;
                }
                else if (c == '.')
                {
                    _lastToken = Tokens.Dot;
                }
                else if (c == ',')
                {
                    _lastToken = Tokens.Comma;
                }
                else if (c == ':')
                {
                    _lastToken = Tokens.Colon;
                }
                else if (IsNumeric(c))
                {
                    _lastToken = ReadNumber();
                }
                else if (c == '\r')
                {
                    bool is2CharNewline = n == '\n';
                    IncrementLine(is2CharNewline);
                }
                else
                {
                    throw new LangException("syntax", "unexpected text in string", string.Empty, _pos.Line, _pos.LineCharPosition);
                }

                var t = new TokenData() { Token = _lastToken, Line = _pos.Line, LineCharPos = _pos.LineCharPosition, Pos = pos };
                tokens.Add(t);

                // Single char symbol - char advancement was not made.
                if ( (t.Token.Kind == TokenKind.Symbol || t.Token.Type == TokenTypes.WhiteSpace) && _pos.Pos == pos  )
                    ReadChar();

                // Before returning, set the next line char position.
                //if (_pos.LineCharPosition != -1 && _lastToken != null && !string.IsNullOrEmpty(_lastToken.Text))
                //{
                //    _pos.LineCharPosition += _lastToken.Text.Length;
                //}
                c = _pos.CurrentChar;
                n = PeekChar();
            }
            return tokens;
        }
        #endregion
    }
}
