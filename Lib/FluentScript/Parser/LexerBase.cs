using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ComLib.Lang
{
    /// <summary>
    /// The result of a scan for a specific token
    /// </summary>
    public class ScanTokenResult
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="success"></param>
        /// <param name="text"></param>
        public ScanTokenResult(bool success, string text)
            : this(success, text, 0)
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="success"></param>
        /// <param name="text">The parsed text</param>
        /// <param name="totalNewLines">The total number of new lines</param>
        public ScanTokenResult(bool success, string text, int totalNewLines)
        {
            Success = success;
            Text = text;
            Lines = totalNewLines;
        }


        /// <summary>
        /// Whether or not the token was properly present
        /// </summary>
        public readonly bool Success;


        /// <summary>
        /// The text of the token.
        /// </summary>
        public readonly string Text;


        /// <summary>
        /// Number of lines parsed.
        /// </summary>
        public int Lines;
    }



    /// <summary>
    /// Base class for the language lexer... this does not know anything about tokens.
    /// This class only has methods to readchars, peekchars, read words, numbers etc.
    /// </summary>
    public class LexerBase
    {
        /// <summary>
        /// Stores the lexical position
        /// </summary>
        public class LexerState
        {
            /// <summary>
            /// Current position in text.
            /// </summary>
            public int Pos;


            /// <summary>
            /// Line number
            /// </summary>
            public int Line;


            /// <summary>
            /// Line char position.
            /// </summary>
            public int LineCharPosition;


            /// <summary>
            /// The source code.
            /// </summary>
            public string Text;


            /// <summary>
            /// The last char parsed.
            /// </summary>
            public char LastChar; 


            /// <summary>
            /// The current char
            /// </summary>
            public char CurrentChar;


            /// <summary>
            /// The next char.
            /// </summary>
            public char NextChar;
        }


        /// <summary>
        /// Single quote
        /// </summary>
        protected const char SQUOTE = '\'';


        /// <summary>
        /// Double quote
        /// </summary>
        protected const char DQUOTE = '"';


        /// <summary>
        /// Space
        /// </summary>
        protected const char SPACE = ' ';


        /// <summary>
        /// Tab
        /// </summary>
        protected const char TAB = '\t';


        /// <summary>
        /// End char
        /// </summary>
        protected const char END_CHAR = ' ';


        /// <summary>
        /// The escape character. e.g. \
        /// </summary>
        protected char _escapeChar;
        
        
        /// <summary>
        /// The current state of the lexers position in the source text.
        /// </summary>
        protected LexerState _pos;
        

        /// <summary>
        /// White space character lookups
        /// </summary>
        protected IDictionary<char, char> _whiteSpaceChars;


        /// <summary>
        /// The index position of the last char in the source text.
        /// </summary>
        internal int LAST_POSITION;


        /// <summary>
        /// The current line number.
        /// </summary>
        public int LineNumber { get { return _pos.Line; } }


        /// <summary>
        /// The char position on the current line.
        /// </summary>
        public int LineCharPos { get { return _pos.LineCharPosition; } }


        /// <summary>
        /// Get the positional state of the lexer
        /// </summary>
        public LexerState State { get { return _pos; } }



        #region Init and Resets
        /// <summary>
        /// Initialize using the supplied parameters.
        /// </summary>
        /// <param name="text">Text to read.</param>
        /// <param name="escapeChar">Escape character.</param>
        /// <param name="tokens">Array with tokens.</param>
        /// <param name="whiteSpaceTokens">Array with whitespace tokens.</param>
        public void Init(string text, char escapeChar, char[] tokens, char[] whiteSpaceTokens)
        {
            Tokens.Default();
            Reset();
            _pos.Text = text;
            LAST_POSITION = _pos.Text.Length - 1;
            _escapeChar = escapeChar;
            _whiteSpaceChars = ToDictionary(whiteSpaceTokens);
            ReadChar();
        }


        /// <summary>
        /// Sets the current position
        /// </summary>
        /// <param name="pos"></param>
        internal void SetPosition(int pos)
        {
            // Pos can never be more than 1 + last index position.
            // e.g. "common"
            // 1. length = 6
            // 2. LAST_POSITION = 5;
            // 3. _state can not be more than 6. 6 indicating that it's past end
            // 4. _state == 5 Indicating it's at end.
            if (pos >= LAST_POSITION) throw new Lang.LangException("Lexical Error", "Can not set position to : " + pos, "", -1, -1);
            if (pos < 0) throw new Lang.LangException("Lexical Error", "Can not set position before 0 : " + pos, "", -1, -1);

            _pos.Pos = pos;
            _pos.CurrentChar = _pos.Text[_pos.Pos];
        }


        /// <summary>
        /// Reset reader for parsing again.
        /// </summary>
        public void Reset()
        {
            _pos = new LexerState();
            _pos.Pos = -1;
            _pos.Line = 1;
            _pos.Text = string.Empty;
            _whiteSpaceChars = new Dictionary<char, char>();
            _escapeChar = '\\';
        }


        /// <summary>
        /// Resets the position.
        /// </summary>
        /// <param name="pos"></param>
        public void ResetPos(int pos)
        {
            ResetTo(pos);
        }


        /// <summary>
        /// Resets the scanner position to 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="handleNewLine"></param>
        public void ResetTo(int pos, bool handleNewLine = true)
        {
            if (pos >= 0)
            {
                _pos.Pos = pos;
                _pos.CurrentChar = _pos.Text[pos];
                if (handleNewLine)
                {
                    if (_pos.CurrentChar == '\n' && (pos - 1 >= 0) && _pos.Text[pos - 1] == '\r')
                    {
                        _pos.Pos--;
                        _pos.CurrentChar = _pos.Text[pos];
                    }
                }
            }
        }
        #endregion



        #region Position Checks
        /// <summary>
        /// Determine if the end of the text input has been reached.
        /// </summary>
        /// <returns>True if the end of the stream has been reached.</returns>
        public bool IsEnded()
        {
            return _pos.Pos >= _pos.Text.Length;
        }


        /// <summary>
        /// Determine if at last char.
        /// </summary>
        /// <returns>True if the last character is the current character.</returns>
        public bool IsAtEnd()
        {
            return _pos.Pos == LAST_POSITION;
        }
        #endregion



        #region Char Operations
        /// <summary>
        /// Returns the char at current position + 1.
        /// </summary>
        /// <returns>Next char or string.empty if end of text.</returns>
        public char PeekChar()
        {
            // Validate.
            if (_pos.Pos >= LAST_POSITION)
                return char.MinValue;

            _pos.NextChar = _pos.Text[_pos.Pos + 1];
            return _pos.NextChar;
        }


        /// <summary>
        /// Returns the nth char from the current char index
        /// </summary>
        /// <param name="countFromCurrentCharIndex">Number of characters from the current char index</param>
        /// <returns>Single char as string</returns>
        public char PeekCharAt(int countFromCurrentCharIndex)
        {
            // Validate.
            if (_pos.Pos + countFromCurrentCharIndex > LAST_POSITION)
                return END_CHAR;

            return _pos.Text[_pos.Pos + countFromCurrentCharIndex];
        }


        /// <summary>
        /// Returns the chars starting at current position + 1 and
        /// including the <paramref name="count"/> number of characters.
        /// </summary>
        /// <param name="count">Number of characters.</param>
        /// <returns>Range of chars as string or string.empty if end of text.</returns>
        public string PeekChars(int count)
        {
            // Validate.
            if (_pos.Pos + count > LAST_POSITION)
                return string.Empty;

            return _pos.Text.Substring(_pos.Pos + 1, count);
        }


        /// <summary>
        /// Read the next char.
        /// </summary>
        /// <returns>Character read.</returns>
        public char ReadChar()
        {
            // NEVER GO PAST 1 INDEX POSITION AFTER CHAR
            if (_pos.Pos > LAST_POSITION) return END_CHAR;

            _pos.Pos++;
            _pos.LineCharPosition++;

            // Still valid?
            if (_pos.Pos <= LAST_POSITION)
            {
                _pos.LastChar = _pos.CurrentChar;
                _pos.CurrentChar = _pos.Text[_pos.Pos];
                return _pos.CurrentChar;
            }
            _pos.CurrentChar = END_CHAR;
            return END_CHAR;
        }


        /// <summary>
        /// Moves forward by count chars.
        /// </summary>
        /// <param name="count"></param>
        public void MoveChars(int count)
        {
            // Pos can never be more than 1 + last index position.
            // e.g. "common"
            // 1. length = 6
            // 2. LAST_POSITION = 5;
            // 3. _state can not be more than 6. 6 indicating that it's past end
            // 4. _state == 5 Indicating it's at end.
            if (_pos.Pos > LAST_POSITION && count > 0) return;

            // Move past end? Move it just 1 position more than last index.
            if (_pos.Pos + count > LAST_POSITION)
            {
                _pos.Pos = LAST_POSITION + 1;
                _pos.LineCharPosition += count;
                _pos.CurrentChar = END_CHAR;
                return;
            }

            // Can move forward count chars
            _pos.Pos += count;
            _pos.LineCharPosition += count;
            _pos.CurrentChar = _pos.Text[_pos.Pos];
        }

        /// <summary>
        /// Consume all white space.
        /// This works by checking the next char against
        /// the chars in the dictionary of chars supplied during initialization.
        /// </summary>
        /// <param name="readFirst">True to read a character
        /// before consuming the whitepsace.</param>
        /// <param name="setPosAfterWhiteSpace">True to move position to after whitespace</param>
        public void ConsumeWhiteSpace(bool readFirst, bool setPosAfterWhiteSpace = true)
        {
            if (readFirst) ReadChar();

            bool matched = false;
            while (_pos.Pos <= LAST_POSITION)
            {
                if (!_whiteSpaceChars.ContainsKey(_pos.CurrentChar))
                {
                    matched = true;
                    break;
                }
                ReadChar();
            }

            // At this point the pos is already after token.
            // If matched and need to set at end of token, move back 1 char
            if (matched && !setPosAfterWhiteSpace) MoveChars(-1);
        }
        #endregion



        #region Scanning Operations
        /// <summary>
        /// Reads an identifier where legal chars for the identifier are [$ . _ a-z A-Z 0-9]
        /// </summary>
        /// <param name="advanceFirst"></param>
        /// <param name="setPosAfterToken">True to move position to after id, otherwise 2 chars past</param>
        /// <returns></returns>
        public ScanTokenResult ScanId(bool advanceFirst, bool setPosAfterToken = true)
        {
            // while for function
            var buffer = new StringBuilder();
            if (advanceFirst) ReadChar();

            bool matched = false;
            bool valid = true;
            while (_pos.Pos <= LAST_POSITION)
            {
                if (('a' <= _pos.CurrentChar && _pos.CurrentChar <= 'z') ||
                        ('A' <= _pos.CurrentChar && _pos.CurrentChar <= 'Z') ||
                         _pos.CurrentChar == '$' || _pos.CurrentChar == '_' ||
                        ('0' <= _pos.CurrentChar && _pos.CurrentChar <= '9')
                   )
                    buffer.Append(_pos.CurrentChar);
                else
                {
                    matched = true;
                    valid = false;
                    break;
                }
                ReadChar();
                if (_pos.Pos < LAST_POSITION)
                    _pos.NextChar = _pos.Text[_pos.Pos + 1];
            }
            // Either 
            // 1. Matched the token
            // 2. Did not match but valid && end_of_file
            bool success = matched || (valid && _pos.Pos > LAST_POSITION);

            // At this point the pos is already after token.
            // If matched and need to set at end of token, move back 1 char
            if (success && !setPosAfterToken) MoveChars(-1);

            return new ScanTokenResult(success, buffer.ToString());
        }


        /// <summary>
        /// Reads a number +/-?[0-9]*.?[0-9]*
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance position first</param>
        /// <param name="setPosAfterToken">True to move position to end space, otherwise past end space.</param>
        /// <returns>Contents of token read.</returns>
        public ScanTokenResult ScanNumber(bool advanceFirst, bool setPosAfterToken = true)
        {
            string sign = "";
            if (advanceFirst) ReadChar();
            if (_pos.CurrentChar == '+' || _pos.CurrentChar == '-') { sign = _pos.CurrentChar.ToString(); ReadChar(); }

            // while for function
            var buffer = new StringBuilder();
            if (advanceFirst) ReadChar();

            bool matched = false;
            bool valid = true;
            bool handledDecimal = false;
            while (_pos.Pos <= LAST_POSITION)
            {
                if (!handledDecimal && _pos.CurrentChar == '.')
                {
                    buffer.Append(_pos.CurrentChar);
                    handledDecimal = true;
                }
                else if ('0' <= _pos.CurrentChar && _pos.CurrentChar <= '9')
                    buffer.Append(_pos.CurrentChar);
                else
                {
                    matched = true;
                    valid = false;
                    break;
                }
                ReadChar();
                if (_pos.Pos < LAST_POSITION)
                    _pos.NextChar = _pos.Text[_pos.Pos + 1];
            }
            // Either 
            // 1. Matched the token
            // 2. Did not match but valid && end_of_file
            bool success = matched || (valid && _pos.Pos > LAST_POSITION);

            // At this point the pos is already after token.
            // If matched and need to set at end of token, move back 1 char
            if (success && !setPosAfterToken) MoveChars(-1);

            //return new ScanTokenResult(success, buffer.ToString());            
            var text = buffer.ToString();

            var finalresult = new ScanTokenResult(success, sign + text);
            finalresult.Lines = 0;
            return finalresult;
        }


        /// <summary>
        /// Read token until endchar
        /// </summary>
        /// <param name="quoteChar">char representing quote ' or "</param>
        /// <param name="escapeChar">Escape character for quote within string.</param>
        /// <param name="advanceFirst">True to advance position first before reading string.</param>
        /// <param name="setPosAfterToken">True to move position to end quote, otherwise past end quote.</param>
        /// <returns>Contents of token read.</returns>
        public ScanTokenResult ScanCodeString(char quoteChar, char escapeChar = '\\', bool advanceFirst = true, bool setPosAfterToken = true)
        {
            // "name" 'name' "name\"s" 'name\'"
            var buffer = new StringBuilder();
            int totalNewLines = 0;
            char curr = advanceFirst ? ReadChar() : _pos.CurrentChar;
            char next = PeekChar();
            bool matched = false;
            while (_pos.Pos <= LAST_POSITION)
            {
                // End string " or '
                if (curr == quoteChar)
                {
                    matched = true;
                    MoveChars(1);
                    break;
                }
                // Not an \ for escaping so just append.
                else if (curr != escapeChar)
                {
                    if (curr == '\r') totalNewLines++;

                    buffer.Append(curr);
                }
                // Escape \
                else if (curr == escapeChar)
                {
                    if (next == quoteChar) buffer.Append(quoteChar);
                    else if (next == '\\') buffer.Append("\\");
                    else if (next == 'r') buffer.Append('\r');
                    else if (next == 'n') buffer.Append('\n');
                    else if (next == 't') buffer.Append('\t');
                    MoveChars(1);
                }

                curr = ReadChar(); next = PeekChar();
            }
            // At this point the pos is already after token.
            // If matched and need to set at end of token, move back 1 char
            if (matched && !setPosAfterToken) MoveChars(-1);

            return new ScanTokenResult(matched, buffer.ToString(), totalNewLines);
        }


        /// <summary>
        /// Reads until the 2 chars are reached.
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance curr position first </param>
        /// <param name="setPosAfterToken">Whether or not to advance to position after chars</param>
        /// <returns>String read.</returns>
        public ScanTokenResult ScanUri(bool advanceFirst, bool setPosAfterToken = true)
        {
            var buffer = new StringBuilder();
            var currentChar = advanceFirst ? ReadChar() : _pos.CurrentChar;
            if (currentChar == SPACE || currentChar == TAB)
                return new ScanTokenResult(true, string.Empty);

            var nextChar = PeekChar();
            bool matched = false;
            while (_pos.Pos <= LAST_POSITION)
            {
                buffer.Append(currentChar);
                if (nextChar == SPACE || nextChar == TAB || nextChar == '(' || nextChar == ')'
                    || nextChar == ',' || nextChar == ';' || nextChar == '[' || nextChar == ']'
                    || nextChar == '\r' || nextChar == '\n' || nextChar == '\t')
                {
                    matched = true;
                    break;
                }
                currentChar = ReadChar();
                nextChar = PeekChar();
            }
            string text = buffer.ToString();
            if (matched && setPosAfterToken)
            {
                MoveChars(1);
            }

            return new ScanTokenResult(matched, text);
        }


        /// <summary>
        /// Reads entire line from curr position
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance curr position first </param>
        /// <param name="setPosAfterToken">Whether or not to move curr position to starting of new line or after</param>
        /// <returns>String read.</returns>
        public ScanTokenResult ScanToNewLine(bool advanceFirst, bool setPosAfterToken = true)
        {
            // while for function
            var buffer = new StringBuilder();
            if (advanceFirst) ReadChar();

            bool matched = false;
            bool is2CharNewLine = false;
            while (_pos.Pos <= LAST_POSITION)
            {
                if (_pos.CurrentChar == '\r')
                {
                    char nextChar = PeekChar();
                    is2CharNewLine = nextChar == '\n';
                    matched = true;
                    break;
                }
                else
                    buffer.Append(_pos.CurrentChar);

                ReadChar();
            }
            // Either 
            // 1. Matched the token
            // 2. Did not match but valid && end_of_file
            bool success = matched || _pos.Pos > LAST_POSITION;

            if (success)
            {
                if (!setPosAfterToken)
                    MoveChars(-1);
            }

            return new ScanTokenResult(success, buffer.ToString());
        }


        /// <summary>
        /// Reads text up the position supplied.
        /// </summary>
        /// <param name="from1CharForward"></param>
        /// <param name="endPos"></param>
        /// <param name="setPosAfterToken">Whether or not to set the position after the token.</param>
        /// <returns></returns>
        public ScanTokenResult ScanToPosition(bool from1CharForward, int endPos, bool setPosAfterToken = true)
        {
            int start = from1CharForward
                      ? _pos.Pos + 1
                      : _pos.Pos;

            int length = (endPos - start) + 1;
            string word = _pos.Text.Substring(start, length);

            // Update the position and 
            MoveChars(endPos - _pos.Pos);
            if (setPosAfterToken)
                MoveChars(1);
            return new ScanTokenResult(true, word);
        }


        /// <summary>
        /// Read the next word
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance first</param>
        /// <param name="setPosAfterToken">Whether or not to set the position after the token.</param>
        /// <param name="extra1">An extra character that is allowed to be part of the word in addition to the allowed chars</param>
        /// <param name="extra2">A second extra character that is allowed to be part of word in addition to the allowed chars</param>        
        /// <returns></returns>
        public ScanTokenResult ScanWordUntilChars(bool advanceFirst, bool setPosAfterToken = true, char extra1 = char.MinValue, char extra2 = char.MinValue)
        {
            // while for function
            var buffer = new StringBuilder();
            if (advanceFirst) ReadChar();

            bool matched = false;
            bool valid = true;
            bool hasExtra1 = extra1 != char.MinValue;
            bool hasExtra2 = extra2 != char.MinValue;

            while (_pos.Pos <= LAST_POSITION)
            {
                var c = _pos.CurrentChar;
                if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') ||
                     ('0' <= c && c <= '9') || (c == '_' || c == '-'))
                {
                    buffer.Append(c);
                }
                else if ((hasExtra1 && c == extra1) || (hasExtra2 && c == extra2))
                    buffer.Append(c);
                else
                {
                    matched = true;
                    valid = false;
                    break;
                }
                ReadChar();
                if (_pos.Pos < LAST_POSITION)
                    _pos.NextChar = _pos.Text[_pos.Pos + 1];
            }
            // Either 
            // 1. Matched the token
            // 2. Did not match but valid && end_of_file
            bool success = matched || (valid && _pos.Pos > LAST_POSITION);

            // At this point the pos is already after token.
            // If matched and need to set at end of token, move back 1 char
            if (success && !setPosAfterToken) MoveChars(-1);

            return new ScanTokenResult(success, buffer.ToString());
        }


        /// <summary>
        /// Reads until the 2 chars are reached.
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance curr position first </param>
        /// <param name="first">The first char expected</param>
        /// <param name="second">The second char expected</param>
        /// <param name="setPosAfterToken">Whether or not to advance to position after chars</param>
        /// <returns>String read.</returns>
        public ScanTokenResult ScanUntilChars(bool advanceFirst, char first, char second, bool setPosAfterToken = true)
        {
            // while for function
            var buffer = new StringBuilder();
            if (advanceFirst) ReadChar();

            bool matched = false;
            bool valid = true;
            int initialNewLines = _pos.Line;
            while (_pos.Pos <= LAST_POSITION)
            {
                if (_pos.CurrentChar == '\r')
                {
                    char nextChar = PeekChar();
                    var is2CharNewLine = nextChar == '\n';
                    IncrementLine(is2CharNewLine);
                    if (is2CharNewLine) buffer.Append("\r\n");
                    else buffer.Append("\n");
                }
                else if(_pos.CurrentChar == first && _pos.NextChar == second)
                {
                    matched = true;
                    valid = false;
                    break;
                }
                else
                {
                    buffer.Append(_pos.CurrentChar);
                    ReadChar();
                }                
                PeekChar();
                if (_pos.Pos < LAST_POSITION)
                    _pos.NextChar = _pos.Text[_pos.Pos + 1];
            }
            // Either 
            // 1. Matched the token
            // 2. Did not match but valid && end_of_file
            bool success = matched || (valid && _pos.Pos > LAST_POSITION);

            // At this point the pos is already after token.
            // If matched and need to set at end of token, move back 1 char
            if (success && !setPosAfterToken)
            {
                MoveChars(-1);
            }
            else if (success && setPosAfterToken)
            {
                MoveChars(2);
            }

            int totalNewLines = _pos.Line - initialNewLines;
            return new ScanTokenResult(success, buffer.ToString(), totalNewLines);
        }


        /// <summary>
        /// Reads a word which must not have space in it and must have space/tab before and after
        /// </summary>
        /// <param name="validChars">Dictionary to check against valid chars.</param>
        /// <param name="advanceFirst">Whether or not to advance position first</param>
        /// <param name="setPosAfterToken">True to move position to end space, otherwise past end space.</param>
        /// <returns>Contents of token read.</returns>
        public ScanTokenResult ScanChars(IDictionary<char, bool> validChars, bool advanceFirst, bool setPosAfterToken = true)
        {
            // while for function
            var buffer = new StringBuilder();
            if (advanceFirst) ReadChar();

            bool matched = false;
            bool valid = true;
            while (_pos.Pos <= LAST_POSITION)
            {
                if (validChars.ContainsKey(_pos.CurrentChar))
                    buffer.Append(_pos.CurrentChar);
                else
                {
                    matched = true;
                    valid = false;
                    break;
                }
                ReadChar();
                if (_pos.Pos < LAST_POSITION)
                    _pos.NextChar = _pos.Text[_pos.Pos + 1];
            }

            // At this point the pos is already after token.
            // If matched and need to set at end of token, move back 1 char
            if (matched && !setPosAfterToken) MoveChars(-1);

            // Either 
            // 1. Matched the token
            // 2. Did not match but valid && end_of_file
            bool success = matched || (valid && _pos.Pos > LAST_POSITION);
            return new ScanTokenResult(success, buffer.ToString());
        }
        #endregion



        #region Misc Helpers
        /// <summary>
        /// Increments the line number
        /// </summary>
        /// <param name="is2CharNewLine"></param>
        protected virtual void IncrementLine(bool is2CharNewLine)
        {
            int count = is2CharNewLine ? 2 : 1;
            MoveChars(count);
            _pos.Line++;
            _pos.LineCharPosition = 1;
        }
        #endregion



        #region Character checks
        /// <summary>
        /// Whether the char is a valid char for an identifier.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsIdStartChar(int c)
        {
            return c == '_' || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z');
        }


        /// <summary>
        /// Whether char is an operator.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsOp(char c)
        {
            if (c == '*' || c == '/' || c == '+' || c == '-' || c == '%' ||
                c == '<' || c == '>' || c == '!' || c == '=' ||
                c == '&' || c == '|')
                return true;

            return false;

            // Avoid 3 function calls.
            //return IsMathOp(c) || IsCompareOp(c) || IsLogicOp(c);
        }


        /// <summary>
        /// Whether char is a math operator * / + - %
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsMathOp(char c)
        {
            return c == '*' || c == '/' || c == '+' || c == '-' || c == '%';
        }


        /// <summary>
        /// Whether char is a logical operator 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsLogicOp(char c)
        {
            return c == '&' || c == '|';
        }


        /// <summary>
        /// Whether char is a compare operator 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsCompareOp(char c)
        {
            return c == '<' || c == '>' || c == '!' || c == '=';
        }


        /// <summary>
        /// Whether or not the char is a numeric char.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsNumeric(char c)
        {
            return c == '.' || (c >= '0' && c <= '9');
        }


        /// <summary>
        /// Whether char is a quote for start of strings.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsStringStart(char c)
        {
            return c == DQUOTE || c == SQUOTE;
        }


        /// <summary>
        /// Whether char is a whitespace or tab.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsWhiteSpace(char c)
        {
            return c == SPACE || c == TAB;
        }
        #endregion



        #region Private
        /// <summary>
        /// Check if all of the items in the collection satisfied by the condition.
        /// </summary>
        /// <typeparam name="T">Type of items.</typeparam>
        /// <param name="items">List of items.</param>
        /// <returns>Dictionary of items.</returns>
        private static IDictionary<T, T> ToDictionary<T>(IList<T> items)
        {
            IDictionary<T, T> dict = new Dictionary<T, T>();
            foreach (T item in items)
            {
                dict[item] = item;
            }
            return dict;
        } 
        #endregion
    }
}
