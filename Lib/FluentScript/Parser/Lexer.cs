using System;
using System.Collections.Generic;
using System.Text;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>


namespace Fluentscript.Lib.Parser
{
    public class LexerDiagnosticData
    {
        public int TotalWhiteSpaceTokens;
        public int TotalNewLineTokens;
        public int TotalTokens;

        public void Reset()
        {
            TotalNewLineTokens = 0;
            TotalTokens = 0;
            TotalWhiteSpaceTokens = 0;
        }
    }


    /// <summary>
    /// Converts script from a series of characters into a series of tokens.
    /// Main method is NextToken(), PeekToken(), it internally uses and exposes 
    /// the scanner for char/text based access the the source code as well.
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
    public class Lexer
    {
        public LexerDiagnosticData DiagnosticData = new LexerDiagnosticData();

        #region Private members
        private Context _ctx;      
        private Token _lastToken;
        private TokenData _lastTokenData;
        private TokenData _endTokenData;
        private int _tokenIndex = -1;
        private bool _hasReplacementsOrRemovals = false;
        private char _interpolatedStartChar = '#';
        private Scanner _scanner;
        
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
        public Lexer(string text)
        {
            this._ctx = new Context();
            this.Init(text);
        }


        /// <summary>
        /// Initialize with script.
        /// </summary>
        /// <param name="script"></param>
        public void Init(string script)
        {
            this._scanner = new Scanner();
            this._scanner.Init(script, '\\', new char[] { '\'', '"' }, new char[] { ' ', '\t' });
            this.LAST_POSITION = this._scanner.LAST_POSITION;
            this.State = this._scanner.State;
            this.Scanner = this._scanner;
        }


        /// <summary>
        /// The context of the program.
        /// </summary>
        public void SetContext(Context ctx) { _ctx = ctx; }


        /// <summary>
        /// Last char position of script.
        /// </summary>
        public int LAST_POSITION;


        /// <summary>
        /// The scan state
        /// </summary>
        public ScanState State;


        /// <summary>
        /// The instance of the scanner for public use
        /// </summary>
        public Scanner Scanner;


        public IAstVisitor OnDemandEvaluator;

        
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
            var hasJsPlugins = _ctx.PluginsMeta.TotalLex() > 0;
            

            TokenData last = null;
            while (true)
            {               
                var token = NextToken();

                //PerformDiagnostics(token);

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
                    var isNewLine = token.Token == Tokens.NewLine;

                    // 3a. Plugins? 
                    if (!isNewLine && hasJsPlugins && _ctx.PluginsMeta.CanHandleLex(token.Token))
                    {
                        var visitor = this.OnDemandEvaluator;
                        var parsedToken = _ctx.PluginsMeta.ParseLex(visitor);
                    }
                    // 3b. Plugins? 
                    else if (!isNewLine && hasPlugins && _ctx.Plugins.CanHandleLex(token.Token))
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
                        if (Tokens.ContainsKey(replaceVal))
                        {
                            var t = Tokens.GetToken(replaceVal);
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
                    throw new LangException("Syntax Error", "Unexpected token", string.Empty, _scanner.State.Line, _scanner.State.LineCharPosition);
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
            var c = _scanner.State.CurrentChar;
            var n = _scanner.PeekChar();
            //var tokenLengthCalcMode = TokenLengthCalcMode.Direct;

            var pos = _scanner.State.Pos;
            var line = _scanner.State.Line;
            var tokenLength = 0;
            var cpos = _scanner.State.LineCharPosition;
            
            if (_scanner.IsEnded())
            {
                _lastToken = Tokens.EndToken;
            }
            // Empty space.
            else if (c == ' ' || c == '\t')
            {
                _scanner.ConsumeWhiteSpace(false, true);
                _lastToken = Tokens.WhiteSpace;
                tokenLength = (_scanner.State.Pos - pos) + 1;
                //tokenLengthCalcMode = TokenLengthCalcMode.WhiteSpace;
            }
            // Variable
            else if (_scanner.IsIdentStart(c))
            {
                _lastToken = ReadWord();
            }
            // Single line
            else if (c == '/' && n == '/')
            {
                _scanner.MoveChars(2);
                var result = _scanner.ScanToNewLine(false, true);
                //tokenLengthCalcMode = TokenLengthCalcMode.String;
                tokenLength = (_scanner.State.Pos - pos) + 1;
                _lastToken = TokenBuilder.ToComment(false, result.Text);
            }
            // Multi-line
            else if (c == '/' && n == '*')
            {
                _scanner.MoveChars(2);
                var result = _scanner.ScanUntilChars(false, '*', '/', false, true);
                //tokenLengthCalcMode = TokenLengthCalcMode.MultilineComment;
                tokenLength = _scanner.State.LineCharPosition;
                _lastToken = TokenBuilder.ToComment(true, result.Text);
            }
            else if (c == '|' && n != '|')
            {
                _lastToken = Tokens.Pipe;
            }
            // Operator ( Math, Compare, Increment ) * / + -, < < > >= ! =
            else if (_scanner.IsOp(c) == true)
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
            else if ( c== '?')
            {
                _lastToken = Tokens.Question;
            }
            else if (c == '\\')
            {
                _lastToken = Tokens.BackSlash;
            }
            // String literal
            else if (c == '"' || c == '\'')
            {
                _lastToken = ReadString( c == '"');
                //tokenLengthCalcMode = TokenLengthCalcMode.String;
                if (_lastToken.Kind == TokenKind.Multi)
                {
                    tokenLength = (_scanner.State.Pos - pos) -2;
                    string text = _scanner.State.Text.Substring(pos + 1, tokenLength);
                    _lastToken.SetText(text);
                }
                else
                {
                    tokenLength = _lastToken.Text.Length + 2;
                }
            }
            else if (_scanner.IsNumeric(c))
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
            if ((t.Token.Kind == TokenKind.Symbol || t.Token.Type == TokenTypes.Unknown || t.Token.Type == TokenTypes.WhiteSpace) && _scanner.State.Pos == pos)
                _scanner.ReadChar();
            return t;
        }


        #region Peek methods
        /// <summary>
        /// Peeks at the next token.
        /// </summary>
        /// <returns></returns>
        public TokenData PeekToken(bool allowSpace = false)
        {
            // Check if ended
            if (_scanner.State.Pos >= _scanner.State.Text.Length)
            {
                // Store this perhaps?
                if (_endTokenData != null) return _endTokenData;
                
                // Create endToken data.
                _endTokenData = new TokenData() { Token = Tokens.EndToken, Line = _scanner.State.Line, Pos = _scanner.State.Pos, LineCharPos = _scanner.State.LineCharPosition };
                return _endTokenData;             
            }

            var line = _scanner.State.Line;
            var linepos = _scanner.State.LineCharPosition;
            var lastToken = _lastToken;
            var lastTokenData = _lastTokenData;
            var iSc = _interpolatedStartChar;
            var pos = _scanner.State.Pos;
            
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
            _scanner.State.Line = line;
            _scanner.State.LineCharPosition = linepos;
            _lastToken = lastToken;
            _lastTokenData = lastTokenData;
            _interpolatedStartChar = iSc;
            _scanner.ResetPos(pos, true);
            return token;
        }
        #endregion



        #region Token Read methods
        /// <summary>
        /// Read word
        /// </summary>
        /// <returns></returns>
        public Token ReadWord()
        {
            var result = _scanner.ScanId(false, true);

            // true / false / null
            if (Tokens.IsLiteral(result.Text))
                return Tokens.Lookup(result.Text);

            // var / for / while
            if (Tokens.IsKeyword(result.Text))
                return Tokens.Lookup(result.Text);

            return TokenBuilder.ToIdentifier(result.Text);
        }


        /// <summary>
        /// Reads a uri such as http, https, ftp, ftps, www.
        /// </summary>
        /// <returns></returns>
        public Token ReadUri()
        {
            var result = _scanner.ScanUri(false, true);
            return TokenBuilder.ToLiteralString(result.Text);
        }


        /// <summary>
        /// Reads the next word that does not include a space or new line
        /// </summary>
        /// <param name="extra1">An extra character that is allowed to be part of the word in addition to the allowed chars</param>
        /// <param name="extra2">A second extra character that is allowed to be part of word in addition to the allowed chars</param>
        /// <returns></returns>
        public Token ReadCustomWord(char extra1, char extra2)
        {
            var result = _scanner.ScanWordUntilChars(false, true, extra1, extra2);
            return TokenBuilder.ToLiteralString(result.Text);
        }


        /// <summary>
        /// Read number
        /// </summary>
        /// <returns></returns>
        public Token ReadNumber()
        {
            var result = _scanner.ScanNumber(false, true);
            return TokenBuilder.ToLiteralNumber(result.Text);
        }


        /// <summary>
        /// Read an operator
        /// </summary>
        /// <returns></returns>
        public Token ReadOperator()
        {
            var result = _scanner.ScanChars(_opChars, false, true);
            return Tokens.Lookup(result.Text);
        }


        /// <summary>
        /// Reads a string either in quote or double quote format.
        /// </summary>
        /// <returns></returns>
        public Token ReadString(bool handleInterpolation = true)
        {
            var quote = _scanner.State.CurrentChar;
                
            // 1. Starts with either ' or "
            // 2. Handles interpolation "homepage of ${user.name} is ${url}"
            if (!handleInterpolation)
            {
                var result = _scanner.ScanCodeString(quote, true, true);
                if(!result.Success)
                    throw new LangException("Syntax Error", "Unterminated string", string.Empty, _scanner.State.Line, _scanner.State.LineCharPosition);

                return TokenBuilder.ToLiteralString(result.Text);
            }
            return this.ReadInterpolatedString(quote, false, true, true);
        }


        /// <summary>
        /// Reads up to the position supplied.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Token ReadToPosition(int pos)
        {
            var result = _scanner.ScanToPosition(true, pos, true);
            return TokenBuilder.ToLiteralString(result.Text);
        }


        /// <summary>
        /// Reads string upto end of line.
        /// </summary>
        /// <returns></returns>
        public Token ReadLine(bool includeNewLine)
        {
            return this.ReadInterpolatedString(Char.MinValue, true, includeNewLine, true);
        }


        /// <summary>
        /// Reads string upto end of line.
        /// </summary>
        /// <returns></returns>
        public Token ReadLineRaw(bool includeNewLine)
        {
            var result = _scanner.ScanToNewLine(false, true);
            var token = TokenBuilder.ToLiteralString(result.Text);
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
            var curr = _scanner.ReadChar();
            var next = _scanner.PeekChar();
            var matched = false;
            var escapeChar = '\\';
            Token token = null;
            while (_scanner.State.Pos <= _scanner.LAST_POSITION)
            {
                // End string " or '
                if (!readLine && curr == quote)
                {
                    matched = true;
                    _scanner.MoveChars(1);
                    break;
                }
                // End of line.
                if (readLine && ( curr == '\r' || curr == '\n' ))
                {
                    matched = true;
                    if (!includeNewLine) break;
                    var is2CharNewLine = _scanner.ScanNewLine(curr);
                    var newline = is2CharNewLine ? "\r\n" : "\n";
                    buffer.Append(newline);
                    token = Tokens.NewLine;
                    break;
                }
                // Interpolation.
                else if (curr == _interpolatedStartChar && next == '{')
                {
                    // Keep track of interpolations and their start positions.
                    interpolationCount++;
                    int interpolatedStringStartPos = _scanner.State.LineCharPosition + 2;
                    int interpolatedStringLinePos = _scanner.State.Line;

                    // Add any existing text before the interpolation as a token.
                    if (buffer.Length > 0)
                    {
                        string text = buffer.ToString();
                        token = TokenBuilder.ToLiteralString(text);
                        var t = new TokenData() { Token = token, LineCharPos = 0, Line = _scanner.State.Line };
                        allTokens.Add(t);
                        buffer.Clear();
                    }
                    _scanner.MoveChars(1);
                    var tokens = ReadInterpolatedTokens();
                    token = TokenBuilder.ToInterpolated(string.Empty, tokens);
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
                    var result = _scanner.ScanEscape(quote, false);
                    buffer.Append(result.Text);
                    _scanner.MoveChars(1);
                }

                curr = _scanner.ReadChar();
                next = _scanner.PeekChar();
            }
            
            // Error: Unterminated string constant.
            if (!matched && !readLine && _scanner.State.Pos >= _scanner.LAST_POSITION)
            {
                throw new LangException("Syntax Error", "Unterminated string", string.Empty, _scanner.State.Line, _scanner.State.LineCharPosition);
            }

            // At this point the pos is already after token.
            // If matched and need to set at end of token, move back 1 char
            if (matched && !setPositionAfterToken) _scanner.MoveChars(-1);
            if (interpolationCount == 0)
            {
                var text = buffer.ToString();
                return TokenBuilder.ToLiteralString(text);
            }
            if (buffer.Length > 0)
            {
                var text = buffer.ToString();
                token = TokenBuilder.ToLiteralString(text);
                allTokens.Add(new TokenData() { Token = token, LineCharPos = 0, Line = _scanner.State.Line });
            }
            return TokenBuilder.ToInterpolated(string.Empty, allTokens);
        }


        public void SkipUntilPrefixedWord(char prefix, string word)
        {
            _scanner.SkipUntilPrefixedWord(false, prefix, word);
        }
        #endregion



        #region Private methods
        /// <summary>
        /// Increments the line number
        /// </summary>
        /// <param name="is2CharNewLine"></param>
        public void IncrementLine(bool is2CharNewLine)
        {
            _scanner.IncrementLine(is2CharNewLine);
            _lastToken = Tokens.NewLine;
        }


        private List<TokenData> ReadInterpolatedTokens()
        {
            var c = _scanner.ReadChar();
            var n = _scanner.PeekChar();
            var tokens = new List<TokenData>();

            while (c != '}' && !_scanner.IsAtEnd())
            {
                var pos = _scanner.State.Pos;
                // Variable
                if (_scanner.IsIdentStart(c))
                {
                    _lastToken = ReadWord();
                }
                // Empty space.
                else if (c == ' ' || c == '\t')
                {
                    _lastToken = Tokens.WhiteSpace;
                }
                else if (_scanner.IsOp(c) == true)
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
                else if (_scanner.IsNumeric(c))
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
                    throw new LangException("syntax", "unexpected text in string", string.Empty, _scanner.State.Line, _scanner.State.LineCharPosition);
                }

                var t = new TokenData() { Token = _lastToken, Line = _scanner.State.Line, LineCharPos = _scanner.State.LineCharPosition, Pos = pos };
                tokens.Add(t);

                // Single char symbol - char advancement was not made.
                if ( (t.Token.Kind == TokenKind.Symbol || t.Token.Type == TokenTypes.WhiteSpace) && _scanner.State.Pos == pos  )
                    _scanner.ReadChar();
                c = _scanner.State.CurrentChar;
                n = _scanner.PeekChar();
            }
            return tokens;
        }


        private void PerformDiagnostics(TokenData tokenData)
        {
            this.DiagnosticData.TotalTokens++;
            if (tokenData.Token == Tokens.NewLine)
                this.DiagnosticData.TotalNewLineTokens++;
            else if (tokenData.Token == Tokens.WhiteSpace)
                this.DiagnosticData.TotalWhiteSpaceTokens++;
        }
        #endregion
    }
}
