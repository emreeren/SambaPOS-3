using System;
using System.Collections.Generic;
using System.Text;

namespace Fluentscript.Lib.Templating
{
    /// <summary>
    /// Templating class for the javascript like language.
    /// Syntax similar to jquery templates/python-django templates.
    /// </summary>
    public class TemplateEngineDefault : ITemplateEngine
    {
        #region Members
        // Parsing.
        private List<CodeBlock> _buffer;
        private string _script;
        private int _lineNum;
        private int _currentLineStartPos;
        private int _posInLine;
        private int _scriptLength;
        private int _pos;
        
        // Constants
        private char ESCAPE = '%';
        private char TEMPLATE_START = '<';
        private char TEMPLATE_END = '>';
        private char CODE_START = '%';
        private char CODE_END = '%';
        private char EXP_START = '=';
        private char EXP_END = '%';
        private char COMMENT_START = '#';
        private char COMMENT_END = '%';
        private string NEW_LINE = "\"\\r\\n\"";
        private string BUFFER_NAME    = "buffer";

        // State
        private string _lastText = string.Empty;
        private State _lastState = State.Html;    

        // Error handling and tracking.
        private List<string> _errors;
        #endregion


        #region Support Classes
        /// <summary>
        /// State of parsing.
        /// </summary>
        enum State
        {
            Html,
            Expression,
            CodeBlock,
            Comment
        }


        /// <summary>
        /// Codeblock
        /// </summary>
        class CodeBlock
        {
            /// <summary>
            /// What type of text html, expression, codeblock, comment etc.
            /// </summary>
            public State TextType;


            /// <summary>
            /// Content of the codeblock.
            /// </summary>
            public string Content;

            
            /// <summary>
            /// Initialize.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="content"></param>
            public CodeBlock(State type, string content)
            {
                TextType = type;
                Content = content;
            }
        }
        #endregion



        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="script"></param>
        public TemplateEngineDefault(string script)
        {
            Init(script);
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="script"></param>
        public void Init(string script)
        {
            _script = script;
            Reset();            
        }


        /// <summary>
        /// Render using rules similar to Razor.
        /// </summary>
        /// <param name="script">the script</param>
        /// <returns></returns>
        public string Render(string script)
        {
            Init(script);
            return Render();
        }



        /// <summary>
        /// Render using rules similar to Razor.
        /// </summary>
        /// <returns></returns>
        public string Render()
        {
            // Validate.
            if (string.IsNullOrEmpty(_script))
                return string.Empty;

            Reset();
            Interpret();
            var output = BuildFinalText();
            return output;
        }



        private void Interpret()
        {
            // Loop through all characthers in script.
            while (_pos < _scriptLength)
            {
                char c = _script[_pos];
                char n1 = _pos + 1 >= _scriptLength ? ' ' : _script[_pos + 1];
                char n2 = _pos + 2 >= _scriptLength ? ' ' : _script[_pos + 2];

                // <html>
                if (c == TEMPLATE_START && n1 != CODE_START)
                {
                    HandleHtml();
                }
                // <%= expression %>
                else if (c == TEMPLATE_START && n1 == CODE_START && n2 == EXP_START)
                {
                    HandleCodeExpression();
                }
                // <%- comment %>
                else if (c == TEMPLATE_START && n1 == CODE_START && n2 == COMMENT_START)
                {
                    HandleComment();
                }
                // <% code %>
                else if (c == TEMPLATE_START && n1 == CODE_START)
                {
                    HandleCodeBlock();
                }
                // %% escape
                else if (c == TEMPLATE_START && n1 == ESCAPE && n2 == ESCAPE)
                {
                    _lastText += '%';
                    _lastState = State.Html;
                }
                else
                {
                    HandleHtml();
                }
                _pos++;
                _posInLine++;
            }
        }


        /// <summary>
        /// Handles reading of %-  comments and setting the current state.
        /// </summary>
        private void HandleHtml()
        {
            string text = ReadHtml(false, 0);
            
            // prevent excessive newlines \r\n
            // such as 
            //  <div>
            //      <% if (...) { %>
            //      <ul>            
            int lastCodeIndex = _buffer.Count - 1;
            if (lastCodeIndex >= 1)
            {
                CodeBlock last = _buffer[lastCodeIndex];
                CodeBlock secondToLast = _buffer[lastCodeIndex - 1];
                if (secondToLast.TextType == State.Html && last.TextType == State.CodeBlock)
                {
                    if (text == "\\r\\n")
                    {
                        text = string.Empty;
                    }
                    else if (text.StartsWith("\\r\\n"))
                    {
                        text = text.Substring(4);
                    }
                }
            }

            text = "\"" + text + "\"";
            if ((_lastState == State.Html || _lastState == State.Expression) && !string.IsNullOrEmpty(_lastText))
            {
                _lastText += " + " + text;
            }
            else
                _lastText += text;

            _lastState = State.Html;
        }


        /// <summary>
        /// Handles reading of %-  comments and setting the current state.
        /// </summary>
        private void HandleComment()
        {
            if (_lastState == State.Html || _lastState == State.Expression)
                OuputBuffer();

            string comment = ReadUntil(COMMENT_END, TEMPLATE_END, true, 3);
            _buffer.Add(new CodeBlock(State.Comment, "/* " + comment + " */"));
            _lastState = State.Comment;
        }


        /// <summary>
        /// Handles reading of % %>
        /// </summary>
        private void HandleCodeBlock()
        {
            if (_lastState == State.Html || _lastState == State.Expression)
                OuputBuffer();

            string code = ReadCodeBlock(CODE_END, TEMPLATE_END, true, 2);
            _buffer.Add(new CodeBlock(State.CodeBlock, code));
            _lastState = State.CodeBlock;
        }


        /// <summary>
        /// Handles the expression.
        /// </summary>
        private void HandleCodeExpression()
        {
            string code = ReadCodeBlock(EXP_END, TEMPLATE_END, true, 3);

            if ((_lastState == State.Html || _lastState == State.Expression) && !string.IsNullOrEmpty(_lastText))
            {
                _lastText += " + " + code;
            }
            else
                _lastText += code;
            _lastState = State.Expression;
        }


        /// <summary>
        /// Reads until char supplied is found.
        /// </summary>
        /// <returns></returns>
        private string ReadUntil(char expected1, char expected2, bool advanceFirst = true, int advanceCount = 1, bool readToEndOfExpected = true, bool excludeBeginningSpace = true)
        {
            // starting at @ so move 1 char forward
            if(advanceFirst) _pos += advanceCount;

            string text = "";

            // Exclude including the beginning space.
            if (excludeBeginningSpace)
            {
                while (_pos < _scriptLength && _script[_pos] == ' ')
                    _pos++;
            }

            while (_pos < _scriptLength)
            {
                Char c = _script[_pos];
                char n1 = _pos + 1 >= _scriptLength ? ' ' : _script[_pos + 1];
                char n2 = _pos + 2 >= _scriptLength ? ' ' : _script[_pos + 2];

                if (c == '\r' && n1 == '\n')
                {
                    text += "\\r\\n";
                    _pos += 2;
                    RegisterNewLine();
                }
                if (n1 == expected1 && n2 == expected2)
                {
                    text += c;
                    if (readToEndOfExpected) _pos += 2;
                    break;
                }
                else
                {
                    text += c;
                    _pos++;
                }
            }
            return text;
        }

        
        /// <summary>
        /// Reads a code block
        /// </summary>
        /// <param name="expected1">%</param>
        /// <param name="expected2">></param>
        /// <param name="advanceFirst">Whether or not to advance the position first</param>
        /// <param name="advanceCount">How many characthers to advance if advancing position</param>
        /// <param name="readToEndOfExpected">Whether or not to set the char pos to the end of the expected chars if found.</param>
        /// <param name="excludeBeginningSpace">Whether or not to exclude the beginning white space.</param>
        /// <returns></returns>
        private string ReadCodeBlock(char expected1, char expected2, bool advanceFirst = true, int advanceCount = 1, bool readToEndOfExpected = true, bool excludeBeginningSpace = true)
        {
            // starting at @ so move 1 char forward
            if (advanceFirst) _pos += advanceCount;

            string text = "";

            // Exclude including the beginning space.
            if (excludeBeginningSpace)
            {
                while (_pos < _scriptLength && _script[_pos] == ' ')
                    _pos++;
            }

            while (_pos < _scriptLength)
            {
                Char c = _script[_pos];
                char n1 = _pos + 1 >= _scriptLength ? ' ' : _script[_pos + 1];
                char n2 = _pos + 2 >= _scriptLength ? ' ' : _script[_pos + 2];

                if (c == '\r' && n1 == '\n')
                {
                    text += "\r\n";
                    _pos += 2;
                    RegisterNewLine();
                }
                // "abc" - string
                else if (c == '"')
                {
                    text += ReadCodeString(true);
                }
                // %>
                else if (c == expected1 && n1 == expected2)
                {
                    if (readToEndOfExpected) _pos += 1;
                    break;
                }
                else
                {
                    text += c;
                    _pos++;
                }
            }
            return text;
        }


        /// <summary>
        /// Reads until char supplied is found.
        /// </summary>
        /// <returns></returns>
        private string ReadHtml(bool advanceFirst = true, int advanceCount = 1)
        {
            // starting at @ so move 1 char forward
            if (advanceFirst) _pos += advanceCount;
                        
            string text = "";
            while (_pos < _scriptLength)
            {
                Char c = _script[_pos];
                char n1 = _pos + 1 >= _scriptLength ? ' ' : _script[_pos + 1];
                char n2 = _pos + 2 >= _scriptLength ? ' ' : _script[_pos + 2];
                char n3 = _pos + 3 >= _scriptLength ? ' ' : _script[_pos + 3];

                // new line
                if (c == '\r' && n1 == '\n')
                {
                    text += "\\r\\n";
                    _pos += 2;
                    RegisterNewLine();
                }
                // <% - begin code block
                else if (c == TEMPLATE_START && n1 == CODE_START && n2 != CODE_START)
                {
                    _pos--;
                    break;
                }
                // "abc" - string
                else if (c == '"')
                {
                    text += "\\\"";
                    _pos++;
                }
                // <!-- --> html comment
                else if (c == '<' && n1 == '!' && n2 == '-' && n3 == '-')
                {
                    text += ReadHtmlComment();
                }
                // text.
                else
                {
                    text += c;
                    _pos++;
                }
            }
            return text;
        }


        private string ReadCodeString(bool advanceAfterEndQuote = false)
        {
            var text = "\"";
            _pos++;
            while (_pos < _scriptLength)
            {
                Char c = _script[_pos];
                Char n1 = _script[_pos + 1];

                // \\
                if (c == '\\' && n1 == '\\')
                {
                    text += "\\\\";
                    _pos += 2;
                }
                // \"
                else if (c == '\\' && n1 == '"')
                {
                    text += "\\\"";
                    _pos += 2;
                }
                // \r
                else if (c == '\\')
                {
                    text += "\\";
                    _pos++;
                }
                else if (c == '"')
                {
                    text += "\"";
                    if (advanceAfterEndQuote) _pos++;
                    break;
                }
                else
                {
                    text += c;
                    _pos++;
                }
            }
            return text;
        }


        private string ReadHtmlComment()
        {
            _pos += 4;
            var text = "<!-- ";
            while (_pos < _scriptLength)
            {
                Char c1 = _script[_pos];
                if (c1 == '"')
                {
                    text += "\\\"";
                    _pos++;
                }
                else if (c1 == '\\')
                {
                    text += "\\\\";
                    _pos++;
                }
                else if (c1 == '-' && _script[_pos + 1] == '-' && _script[_pos + 2] == '>')
                {
                    text += " -->";
                    _pos += 3;
                    break;
                }
                else
                {
                    text += c1;
                    _pos++;
                }
            }
            return text;
        }


        #region Helper methods
        private void OuputBuffer()
        {
            if (_lastText == NEW_LINE)
            {
                _lastText = string.Empty;
                return;
            }
            _buffer.Add(new CodeBlock(State.Html, _lastText));
            _lastText = string.Empty;
        }


        private void RegisterNewLine()
        {
            _lineNum++;
            _currentLineStartPos = _pos;
            _posInLine = 0;
        }


        private void Reset()
        {
            // Initialize            
            _buffer = new List<CodeBlock>();
            _errors = new List<string>();
            _lineNum = 1;
            _currentLineStartPos = 0;
            _scriptLength = _script.Length;
            _pos = 0;
            _posInLine = 0;
            _lastText = string.Empty;
        }


        private string BuildFinalText()
        {
            if (!string.IsNullOrEmpty(_lastText))
                OuputBuffer();

            var allText = new StringBuilder();
            allText.Append("var " + BUFFER_NAME + " = \"\";" + Environment.NewLine);
            allText.Append(_buffer[0].Content + Environment.NewLine);

            for(int ndx = 1; ndx < _buffer.Count; ndx++)
            {
                CodeBlock blockC = _buffer[ndx];
                string text = blockC.Content;
                if (blockC.TextType == State.Html)
                    text = BUFFER_NAME + " += " + text + ";";
                allText.Append(text + Environment.NewLine);
            }
            var finaltext = allText.ToString();
            return finaltext;
        }
        #endregion
    }
}
