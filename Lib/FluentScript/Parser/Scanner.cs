using System.Collections.Generic;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser
{
    /// <summary>
    /// The result of a scan for a specific token
    /// </summary>
    public class ScanResult
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="success"></param>
        /// <param name="start">The starting position.</param>
        /// <param name="text">The parsed text</param>
        /// <param name="totalNewLines">The total number of new lines</param>
        public ScanResult(bool success, int start, string text, int totalNewLines)
        {
            this.Start = start;
            this.Success = success;
            this.Text = text;
            this.Lines = totalNewLines;
        }


        /// <summary>
        /// Start position.
        /// </summary>
        public readonly int Start;


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
    /// Stores the lexical position
    /// </summary>
    public class ScanState
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


        /// <summary>
        /// Extracts the text from the start to end inclusive of the positions.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public string ExtractInclusive(int start, int end)
        {
            var length = end - start + 1;
            var text = this.Text.Substring(start, length);
            return text;
        }
    }



    /// <summary>
    /// This class scans the source code characther by character for words, numbers, symbols.
    /// It doesn't know anything about tokens like the lexer does. 
    /// The API here is geared toward only getting/checking charaters, scanning words and peeking
    /// and words/numbers etc. 
    /// In this sense it is highly reusable across the c# applications and will be reused 
    /// in the commonlibrary.net project.
    /// </summary>
    public class Scanner
    {
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


        protected const char CARRIAGERETURN = '\r';


        protected const char NEWLINE = '\n';


        /// <summary>
        /// End char
        /// </summary>
        protected const char END_CHAR = char.MinValue;


        /// <summary>
        /// The escape character. e.g. \
        /// </summary>
        protected char _escapeChar;


        /// <summary>
        /// Whitespace characters
        /// </summary>
        protected IDictionary<char, char> _whiteSpaceChars; 


        /// <summary>
        /// The current state of the lexers position in the source text.
        /// </summary>
        protected ScanState _pos;


        /// <summary>
        /// The index position of the last char in the source text.
        /// </summary>
        public int LAST_POSITION;


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
        public ScanState State { get { return _pos; } }



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
        /// Reset reader for parsing again.
        /// </summary>
        public void Reset()
        {
            _pos = new ScanState();
            _pos.Pos = -1;
            _pos.Line = 1;
            _pos.Text = string.Empty;
            _whiteSpaceChars = new Dictionary<char, char>();
            _escapeChar = '\\';
        }


        /// <summary>
        /// Resets the scanner position to 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="handleNewLine"></param>
        public void ResetPos(int pos, bool handleNewLine)
        {
            if (pos > LAST_POSITION) throw new LangException("Lexical Error", "Can not set position to : " + pos, "", -1, -1);
            if (pos < 0) throw new LangException("Lexical Error", "Can not set position before 0 : " + pos, "", -1, -1);

            _pos.Pos = pos;
            _pos.CurrentChar = _pos.Text[pos];
            _pos.LastChar = _pos.Text[pos - 1];
            if (handleNewLine)
            {
                if (_pos.CurrentChar == '\n' && (pos - 1 >= 0) && _pos.Text[pos - 1] == '\r')
                {
                    _pos.Pos--;
                    _pos.CurrentChar = _pos.Text[pos];
                }
            }
        }


        /// <summary>
        /// Updates the current char/next char and line number due to direct changes to position
        /// rather than calling ReadChar(), MoveChars().
        /// </summary>
        public void UpdateLineState(int startPos)
        {
            var currentPos = _pos.Pos;
            if (currentPos <= LAST_POSITION)
            {
                _pos.CurrentChar = _pos.Text[currentPos];
                _pos.LastChar = _pos.Text[currentPos - 1];
            }
            else
            {
                _pos.CurrentChar = _pos.Text[currentPos -1];
                _pos.LastChar = _pos.Text[currentPos - 2];
            }
            var diff = currentPos - startPos;
            _pos.LineCharPosition += diff;
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
                return END_CHAR;

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
        public string PeekMaxChars(int count)
        {
            // Validate... if too much, return remainder of text.
            if (_pos.Pos + count > LAST_POSITION)
            {
                return _pos.Text.Substring(_pos.Pos + 1);
            }

            return _pos.Text.Substring(_pos.Pos + 1, count);
        }


        /// <summary>
        /// Read the next char.
        /// </summary>
        /// <returns>Character read.</returns>
        public char ReadChar()
        {
            // NEVER GO PAST 1 INDEX POSITION AFTER CHAR
            if (_pos.Pos > LAST_POSITION) return char.MinValue;

            _pos.Pos++;
            _pos.LineCharPosition++;

            // Still valid?
            if (_pos.Pos <= LAST_POSITION)
            {
                _pos.LastChar = _pos.CurrentChar;
                _pos.CurrentChar = _pos.Text[_pos.Pos];
                return _pos.CurrentChar;
            }
            _pos.LastChar = _pos.CurrentChar;
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

            var endPos = _pos.Pos + count;
            // Move past end? Move it just 1 position more than last index.
            if (endPos == LAST_POSITION + 1)
            {
                _pos.Pos = LAST_POSITION + 1;
                _pos.LineCharPosition += count;
                _pos.LastChar = _pos.Text[LAST_POSITION];
                _pos.CurrentChar = END_CHAR;
                return;
            }
            if (endPos > LAST_POSITION + 1)
                throw new LangException("Syntax", "Can not move past end position of script", "", _pos.Line, _pos.LineCharPosition);
            
            // Can move forward count chars
            _pos.Pos += count;
            _pos.LineCharPosition += count;
            _pos.LastChar = _pos.Text[_pos.Pos - 1];
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
        public void ConsumeWhiteSpace(bool readFirst, bool setPosAfterWhiteSpace)
        {
            if (readFirst) ReadChar();

            var matched = false;
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


        /// <summary>
        /// Increments the line number
        /// </summary>
        /// <param name="is2CharNewLine"></param>
        public virtual void IncrementLine(bool is2CharNewLine)
        {
            int count = is2CharNewLine ? 2 : 1;
            MoveChars(count);
            _pos.Line++;
            _pos.LineCharPosition = 1;
        }


        /// <summary>
        /// Peeks at the next word that does not include a space.
        /// </summary>
        /// <returns></returns>
        public ScanResult PeekWord(bool advanceFirst)
        {
            var start = advanceFirst ? _pos.Pos + 1 : _pos.Pos;
            var currPos = start;
            var first = true;
            while (currPos <= LAST_POSITION)
            {
                var ch = _pos.Text[currPos];
                var isValidChar = (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ch == '$' || ch == '_'
                                   || (!first && '0' <= ch && ch <= '9')
                                 );
                if (!isValidChar)
                {
                    break;
                }

                currPos++;
                first = false;
            }
            var word = _pos.ExtractInclusive(start, currPos - 1);
            var success = !string.IsNullOrEmpty(word);
            return new ScanResult(success, start, word, 0);
        }


        /// <summary>
        /// Peeks at the next word that does not include a space.
        /// </summary>
        /// <returns></returns>
        public ScanResult PeekNumber(bool advanceFirst)
        {
            var start = advanceFirst ? _pos.Pos + 1 : _pos.Pos;
            var currPos = start;
            var handledDecimal = false;
            while (currPos <= LAST_POSITION)
            {
                var ch = _pos.Text[currPos];
                if (ch == '.')
                {
                    // 1st decimal
                    if (!handledDecimal)
                        handledDecimal = true;

                    // 2nd decimal 
                    else
                        break;
                }
                else if (!('0' <= ch && ch <= '9'))
                {
                    break;
                }
                currPos++;
            }
            var word = _pos.ExtractInclusive(start, currPos - 1);
            var success = !string.IsNullOrEmpty(word);
            return new ScanResult(success, start, word, 0);
        }


        /// <summary>
        /// Reads a word which must not have space in it and must have space/tab before and after
        /// </summary>
        /// <param name="validChars">Dictionary to check against valid chars.</param>
        /// <param name="advanceFirst">Whether or not to advance position first</param>
        /// <param name="setPosAfterToken">True to move position to end space, otherwise past end space.</param>
        /// <returns>Contents of token read.</returns>
        public ScanResult PeekCustomWord(IDictionary<char, bool> validChars, bool advanceFirst)
        {
            var start = advanceFirst ? _pos.Pos + 1 : _pos.Pos;
            var currPos = start;            
            while (currPos <= LAST_POSITION)
            {
                var ch = _pos.Text[currPos];
                if (!validChars.ContainsKey(ch))
                {
                    break;
                }
                currPos++;
            }
            var word = _pos.ExtractInclusive(start, currPos - 1);
            var success = !string.IsNullOrEmpty(word);
            return new ScanResult(success, start, word, 0);
        }


        /// <summary>
        /// Peeks at the next word that does not include a space or new line
        /// </summary>
        /// <param name="expectChar">A char to expect while peeking at the next word.</param>
        /// <param name="maxAdvancesBeforeExpected">The maximum number of advances that the expectChar should appear by</param>        
        /// <param name="extra1">An extra character that is allowed to be part of the word in addition to the allowed chars</param>
        /// <param name="extra2">A second extra character that is allowed to be part of word in addition to the allowed chars</param>
        /// <returns></returns>
        public ScanResult PeekCustomLimitedWord(bool advanceFirst, char expectChar, int maxAdvancesBeforeExpected, char extra1, char extra2)
        {
            var hasExtra1 = extra1 != char.MinValue;
            var hasExtra2 = extra2 != char.MinValue;
            var start = advanceFirst ? _pos.Pos + 1 : _pos.Pos;
            var currPos = start;
            var first = true;
            var found1stChar = false;
            var totalCharsRead = 0;
            while (currPos <= LAST_POSITION)
            {
                var ch = _pos.Text[currPos];
                totalCharsRead++;

                if (ch == expectChar)
                    found1stChar = true;

                if (totalCharsRead >= maxAdvancesBeforeExpected && !found1stChar)
                    break;

                var isValidChar = (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ch == '$' || ch == '_'
                                   || (!first && '0' <= ch && ch <= '9')
                                 );
                if ( !isValidChar && (hasExtra1 && ch != extra1) && (hasExtra2 && ch != extra2) )
                    break;

                currPos++;
                first = false;
            }
            var word = _pos.ExtractInclusive(start, currPos - 1);
            var success = !string.IsNullOrEmpty(word);
            return new ScanResult(success, start, word, 0);
        }
        #endregion



        #region Scanning Operations
        /// <summary>
        /// Reads an identifier where legal chars for the identifier are [$ . _ a-z A-Z 0-9]
        /// </summary>
        /// <param name="advanceFirst"></param>
        /// <param name="setPosAfterToken">True to move position to after id, otherwise 2 chars past</param>
        /// <returns></returns>
        public ScanResult ScanId(bool advanceFirst, bool setPosAfterToken)
        {
            if (advanceFirst) this.ReadChar();

            var start = _pos.Pos;
            var first = true;
            while (_pos.Pos <= LAST_POSITION)
            {
                var ch = _pos.Text[_pos.Pos];
                var isValidChar = (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ch == '$' || ch == '_' 
                                    || (!first && '0' <= ch && ch <= '9')
                                  );
                if(!isValidChar)
                {
                    break;
                }
                _pos.Pos++;
                first = false;
            }
            var text = _pos.ExtractInclusive(start, _pos.Pos - 1);
            if(!setPosAfterToken) this.MoveChars(-1);
            this.UpdateLineState(start);
            var result = new ScanResult(true, start, text, 0);
            return result;
        }


        /// <summary>
        /// Reads a number +/-?[0-9]*.?[0-9]*
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance position first</param>
        /// <param name="setPosAfterToken">True to move position to end space, otherwise past end space.</param>
        /// <returns>Contents of token read.</returns>
        public ScanResult ScanNumber(bool advanceFirst, bool setPosAfterToken)
        {
            if (advanceFirst) this.ReadChar();
            var start = _pos.Pos;
            if (_pos.CurrentChar == '+' || _pos.CurrentChar == '-')
                _pos.Pos++;

            var handledDecimal = false;
            
            // Handles .9, 0.9, 1.9
            while (_pos.Pos <= LAST_POSITION)
            {
                var ch = _pos.Text[_pos.Pos];

                if (ch == '.')
                {
                    // 1st decimal
                    if (!handledDecimal)
                        handledDecimal = true;

                    // 2nd decimal 
                    else
                        break;
                }
                else if (!('0' <= ch && ch <= '9'))
                {
                    break;
                }
                _pos.Pos++;
            }

            var text = _pos.ExtractInclusive(start, _pos.Pos - 1);
            if (!setPosAfterToken) this.MoveChars(-1);
            this.UpdateLineState(start);
            var result = new ScanResult(true, start, text, 0);
            return result;
        }


        /// <summary>
        /// Read token until endchar
        /// </summary>
        /// <param name="quoteChar">char representing quote ' or "</param>
        /// <param name="escapeChar">Escape character for quote within string.</param>
        /// <param name="advanceFirst">True to advance position first before reading string.</param>
        /// <param name="setPosAfterToken">True to move position to end quote, otherwise past end quote.</param>
        /// <returns>Contents of token read.</returns>
        public ScanResult ScanCodeString(char quoteChar, bool advanceFirst, bool setPosAfterToken)
        {
            // "name" 'name' "name\"s" 'name\'"
            var text = "";
            var start = _pos.Pos;
            var initialLineNum = _pos.Line;
            var matched = false;
            if(advanceFirst) this.ReadChar();
            while (_pos.Pos <= LAST_POSITION)
            {
                var ch = _pos.Text[_pos.Pos];
                
                // Case 1: End of string.
                if (ch == quoteChar)
                {
                    matched = true;
                    break;
                }
                // Case 2: New line
                else if (ch == '\r' || ch == '\n')
                {
                    var is2CharNewLine = this.ScanNewLine(ch);
                    text += is2CharNewLine ? "\r\n" : "\n";
                }
                // Case 3: Escape \
                else if (ch == _escapeChar)
                {
                    var result = this.ScanEscape(quoteChar, true);
                    text += result.Text;
                }
                else
                {
                    text += ch;
                    _pos.Pos++;
                    _pos.LineCharPosition++;
                }
            }

            if (setPosAfterToken) this.MoveChars(1);
            var totalNewLines = _pos.Line - initialLineNum;            
            return new ScanResult(matched, start, text, totalNewLines);
        }


        /// <summary>
        /// Reads until the 2 chars are reached.
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance curr position first </param>
        /// <param name="setPosAfterToken">Whether or not to advance to position after chars</param>
        /// <returns>String read.</returns>
        public ScanResult ScanUri(bool advanceFirst, bool setPosAfterToken)
        {
            var ch = advanceFirst ? this.ReadChar() : _pos.CurrentChar;
            var start = _pos.Pos;
            if (ch == SPACE || ch == TAB)
                return new ScanResult(true, start, string.Empty, 0);

            while (_pos.Pos <= LAST_POSITION)
            {
                ch = _pos.Text[_pos.Pos];
                if (ch == SPACE || ch == TAB || ch == '(' || ch == ')'
                    || ch == ',' || ch == ';' || ch == '[' || ch == ']'
                    || ch == '\r' || ch == '\n' || ch == '{' || ch == '}')
                {
                    break;
                }
                _pos.Pos++;
            }
            var text = _pos.ExtractInclusive(start, _pos.Pos - 1);
            if (!setPosAfterToken) this.MoveChars(-1);
            this.UpdateLineState(start); 
            return new ScanResult(true, start, text, 0);
        }


        /// <summary>
        /// Reads entire line from curr position, does not include the newline in result.
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance curr position first </param>
        /// <param name="setPosAfterToken">Whether or not to move curr position to starting of new line or after</param>
        /// <returns>String read.</returns>
        public ScanResult ScanToNewLine(bool advanceFirst, bool setPosAfterToken)
        {
            if (advanceFirst) this.ReadChar();

            var start = _pos.Pos;
            var is2CharNewLine = false;
            while (_pos.Pos <= LAST_POSITION)
            {
                var ch = _pos.Text[_pos.Pos];
                if (ch == '\r')
                {
                    var n1 = this.PeekChar();
                    is2CharNewLine = n1 == '\n';
                    break;
                }
                if(ch == '\n')
                {
                    break;
                }
                _pos.Pos++;
            }
            var text = _pos.ExtractInclusive(start, _pos.Pos - 1);
            if (!setPosAfterToken) this.MoveChars(-1);
            this.UpdateLineState(start);
            return new ScanResult(true, start, text, 0);
        }


        /// <summary>
        /// Reads text up the position supplied.
        /// </summary>
        /// <param name="from1CharForward"></param>
        /// <param name="endPos"></param>
        /// <param name="setPosAfterToken">Whether or not to set the position after the token.</param>
        /// <returns></returns>
        public ScanResult ScanToPosition(bool startAtNextChar, int endPos, bool setPosAfterToken)
        {
            var start = _pos.Pos;
            var actualStartPos = startAtNextChar ? _pos.Pos + 1 : _pos.Pos;
            var word = _pos.ExtractInclusive(actualStartPos, endPos);

            // Update the position and 
            this.ResetPos(endPos, false);
            if (setPosAfterToken) this.ReadChar();
            return new ScanResult(true, actualStartPos, word, 0);
        }


        /// <summary>
        /// Read the next word
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance first</param>
        /// <param name="setPosAfterToken">Whether or not to set the position after the token.</param>
        /// <param name="extra1">An extra character that is allowed to be part of the word in addition to the allowed chars</param>
        /// <param name="extra2">A second extra character that is allowed to be part of word in addition to the allowed chars</param>        
        /// <returns></returns>
        public ScanResult ScanWordUntilChars(bool advanceFirst, bool setPosAfterToken, char extra1, char extra2)
        {
            if (advanceFirst) this.ReadChar();
            var start = _pos.Pos;
            int initialNewLines = _pos.Line;
            var first = true;
            while (_pos.Pos <= LAST_POSITION)
            {
                var ch = _pos.Text[_pos.Pos];
                if ( ('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') ||
                     ('0' <= ch && ch <= '9') ||  ch == '_' || (!first && ch == '-') 
                    || ch == extra1 || ch == extra2 )
                {
                    _pos.Pos++;
                }
                else
                {
                    break;
                }
                first = false;
            }
            var text = _pos.ExtractInclusive(start, _pos.Pos - 1);
            if (!setPosAfterToken) this.MoveChars(-1);
            this.UpdateLineState(start);
            return new ScanResult(true, start, text, 0);
        }


        /// <summary>
        /// Reads until the 2 chars are reached.
        /// </summary>
        /// <param name="advanceFirst">Whether or not to advance curr position first </param>
        /// <param name="first">The first char expected</param>
        /// <param name="second">The second char expected</param>
        /// <param name="setPosAfterToken">Whether or not to advance to position after chars</param>
        /// <returns>String read.</returns>
        public ScanResult ScanUntilChars(bool advanceFirst, char first, char second, bool includeChars, bool setPosAfterToken)
        {
            if (advanceFirst) this.ReadChar();
            var start = _pos.Pos;
            var lineStartPos = _pos.Pos;         
            int initialNewLines = _pos.Line;
            while (_pos.Pos <= LAST_POSITION - 1)
            {
                var ch = _pos.Text[_pos.Pos];
                // Case 1: Matching
                if(ch == first)
                {
                    var n1 = this.PeekChar();
                    if (n1 == second)
                    {
                        _pos.Pos += 2;
                        break;
                    }
                    _pos.Pos++;
                }
                // Case 2: New line
                else if (ch == '\r' || ch == '\n')
                {
                    this.ScanNewLine(ch);
                    lineStartPos = _pos.Pos;
                }
                else
                {
                    _pos.Pos++;
                }
            }
            var endPos = includeChars ? _pos.Pos - 1 : _pos.Pos - 3;
            var text = _pos.ExtractInclusive(start, endPos);
            if (!setPosAfterToken) this.MoveChars(-1);
            this.UpdateLineState(lineStartPos);
            var totalNewLines = _pos.Line - initialNewLines;
            return new ScanResult(true, start, text, totalNewLines);
        }


        /// <summary>
        /// Reads a word which must not have space in it and must have space/tab before and after
        /// </summary>
        /// <param name="validChars">Dictionary to check against valid chars.</param>
        /// <param name="advanceFirst">Whether or not to advance position first</param>
        /// <param name="setPosAfterToken">True to move position to end space, otherwise past end space.</param>
        /// <returns>Contents of token read.</returns>
        public ScanResult ScanChars(IDictionary<char, bool> validChars, bool advanceFirst, bool setPosAfterToken)
        {
            if (advanceFirst) this.ReadChar();
            var start = _pos.Pos;

            while (_pos.Pos <= LAST_POSITION)
            {
                var ch = _pos.Text[_pos.Pos];
                if (!validChars.ContainsKey(ch))
                {
                    break;
                }
                _pos.Pos++;
            }
            var text = _pos.ExtractInclusive(start, _pos.Pos - 1);
            if (!setPosAfterToken) this.MoveChars(-1);
            this.UpdateLineState(start);
            return new ScanResult(true, start, text, 0);
        }


        /// <summary>
        /// Scans an escape character.
        /// </summary>
        /// <param name="quoteChar"></param>
        /// <returns></returns>
        public ScanResult ScanEscape(char quoteChar, bool setPosAfterToken)
        {
            var start = _pos.Pos;
            var text = "";
            var next = this.PeekChar();
            if (next == quoteChar) text += quoteChar;
            else if (next == '\\') text = "\\";
            else if (next == 'r')  text = "\r";
            else if (next == 'n')  text = "\n";
            else if (next == 't')  text = "\t";
            if (setPosAfterToken)
            {
                _pos.Pos += 2;
                _pos.LineCharPosition += 2;
            }
            return new ScanResult(true, start, text, 0);
        }


        /// <summary>
        /// Scans past the new line after tracking line counts
        /// </summary>
        public bool ScanNewLine(char ch)
        {
            var is2CharNewLine = false;
            if (ch == '\r')
            {
                var n1 = this.PeekChar();
                is2CharNewLine = n1 == '\n';
                this.IncrementLine(is2CharNewLine);                
            }
            else if (ch == '\n')
            {
                this.IncrementLine(false);
            }
            return is2CharNewLine;
        }


        public ScanResult SkipUntilPrefixedWord(bool advanceFirst, char prefix, string word)
        {
            if (advanceFirst) this.ReadChar();
            var start = _pos.Pos;
            var checkWord = false;
            var lastWord = string.Empty;
            var buffer = string.Empty;
            var success = false;
            while (_pos.Pos <= LAST_POSITION)
            {
                var ch = _pos.Text[_pos.Pos];
                if (ch == prefix)
                {
                    checkWord = true;
                    _pos.Pos++;
                }
                else if(checkWord )
                {
                    // Case 1: letter
                    if (char.IsLetter(ch))
                        buffer += ch;
                    
                    // Case 2: not a letter after prefix ( so turn off checking word )
                    else if(string.IsNullOrEmpty(buffer))
                        checkWord = false;
                    
                    // Case 3: buffer has letters but current char is not a letter so end.
                    else
                    {
                        checkWord = false;
                        lastWord = buffer;
                        buffer = string.Empty;
                        if(ch == '\r')
                        {
                            var nextchar = this.PeekChar();
                            this.IncrementLine(nextchar == '\n');
                        }
                        if(lastWord == word)
                        {
                            break;
                        }
                    }
                    _pos.Pos++;
                }
                else if (ch == '\r')
                {
                    var nextchar = this.PeekChar();
                    this.IncrementLine(nextchar == '\n');
                }
                else
                {
                    _pos.Pos++;
                }
            }
            success = lastWord == word;
            return new ScanResult(success, start, lastWord, 0);
        }

        #endregion



        #region Character checks
        /// <summary>
        /// Whether the char is a valid char for an identifier.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsIdentStart(int c)
        {
            return c == '_' || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z');
        }


        /// <summary>
        /// Whether char is an operator.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsOp(char c)
        {
            if (c == '*' || c == '/' || c == '+' || c == '-' || c == '%' ||
                c == '<' || c == '>' || c == '!' || c == '=' ||
                c == '&' || c == '|')
                return true;

            return false;
        }


        /// <summary>
        /// Whether or not the char is a numeric char.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsNumeric(char c)
        {
            return c == '.' || (c >= '0' && c <= '9');
        }


        /// <summary>
        /// Wheter or not this the start of the string
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsStringStart(char c)
        {
            return c == DQUOTE || c == SQUOTE;
        }


        /// <summary>
        /// Whether char is a whitespace or tab.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsWhiteSpace(char c)
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
