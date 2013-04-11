using System;
using System.Collections.Generic;

namespace Fluentscript.Lib.Parser.MetaPlugins
{
    /// <summary>
    /// Parses grammar for either a match or the full parsing of plugins.
    /// </summary>
    public class GrammerParser
    {
        private int _pos;
        private int END;
        private string _grammar;
        private List<string> _errors;
        private List<TokenMatch> _matches;
        private List<string> _operatorStack; 
        private int _groupCount;
        private int _orCount;


        public List<TokenMatch> Parse(string grammar)
        {
            // $month:@starttoken.value $day:@number $dayend:( st | nd | rd | th )? ( ','?  $year:@number )?"	            
            _pos = 0;
            _grammar = grammar;
            _errors = new List<string>();
            _operatorStack = new List<string>();
            END = _grammar.Length;
            _matches = new List<TokenMatch>();
            this.DoParse();
            if(_groupCount > 0)
                throw new ArgumentException("Grammar contains extra parenthesis : " + grammar);
            if (_orCount > 0)
                throw new ArgumentException("Grammar contains extra 'or(|)' condition : " + grammar);

            return _matches;
        }


        public int TotalRequired(List<TokenMatch> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;

            if (matches.Count == 1)
                return matches[0].TotalRequired();

            var total = 0;
            for (var ndx = 0; ndx < matches.Count; ndx++)
            {
                total += matches[ndx].TotalRequired();
            }
            return total;
        }


        private void DoParse()
        {
            while (_pos < END)
            {
                var c = _grammar[_pos];

                // Case 1: Begin a token match.
                if (c == '$' || c == '\'')
                {
                    var match = ReadToken();
                    AddMatch(match);
                }
                // Case 2: '@' specific type of word/identifier
                else if ( c == '@')
                {
                    _pos++;
                    var tokenType = ReadWord();
                    var match = new TokenMatch(); 
                    match.TokenType = "@" + tokenType;
                    match.IsRequired = true;
                    AddMatch(match);
                }
                // Case 3: Move past white space.
                else if (c == '\t' || c == ' ')
                {
                    _pos++;
                }
                // Case 4: Last token was optional
                else if (c == '?')
                {
                    _pos++;
                    var last = this.Last();
                    if(_groupCount > 0 && last.IsGroup)
                    {
                        var group = ((TokenGroup) last);
                        last = group.Matches[group.Matches.Count - 1];
                    }
                    last.IsRequired = false;
                }
                // Case 5: "(" starts a grouping of token matches
                else if( c == '(')
                {
                    _pos++;
                    this.BeginGroup();
                }
                else if (c == ')')
                {
                    _pos++;
                    CloseGroup();
                }
                // Case 6: word
                else if (char.IsLetter(c))
                {
                    var word = ReadWord();
                    var match = new TokenMatch();
                    match.Text = word;
                    match.IsRequired = true;
                    AddMatch(match);
                }
                // Case 7: |
                else if( c == '|')
                {
                    _orCount++;
                    _operatorStack.Add("|");
                    _pos++;
                }
                // Case 8: escape \
                else if( c == '\\')
                {
                    _pos++;
                    var escapedChar = _grammar[_pos];
                    var match = new TokenMatch();
                    match.Text = escapedChar.ToString();
                    match.IsRequired = true;
                    AddMatch(match);

                    // Move past escaped char
                    _pos++;
                }
                // Case 9: #reference
                else if (c == '#')
                {                    
                    var match = ReadToken();
                    match.Ref = match.Name;
                    match.Name = null;
                    AddMatch(match);
                }
                else
                    _pos++;
            }
        }


        private string LastOperator()
        {
            var lastIndex = _operatorStack.Count - 1;
            if (lastIndex < 0) return string.Empty;

            var lastOp = _operatorStack[lastIndex];
            return lastOp;
        }


        private void RemoveLastOperator()
        {
            var lastIndex = _operatorStack.Count - 1;
            _operatorStack.RemoveAt(lastIndex);
        }


        private void BeginGroup()
        {
            var group = new TokenGroup();
            group.IsRequired = true;
            _matches.Add(group);
            _groupCount++;
            _operatorStack.Add("(");
        }

        
        private void CloseGroup()
        {
            _groupCount--;
            this.ProcessOperatorForGroup();
            this.ProcessOperatorForOr();
        }


        private void AddMatch(TokenMatch match)
        {
            var matches = this.GetMatchCollection();
            if(matches != null)
                matches.Add(match);
            this.ProcessOperatorForOr();
        }


        private List<TokenMatch> GetMatchCollection()
        {
            var last = Last();
            List<TokenMatch> matchCollection = null;

            // Case 1: Last one is a group.
            if (_groupCount > 0 && last != null && last.IsGroup)
            {
                matchCollection = ((TokenGroup) last).Matches;
            }
            // Case 2: Root level list of matches.
            else
            {
                matchCollection = _matches;
            }
            return matchCollection;
        }


        private TokenMatch Last()
        {
            if (_matches.Count == 0) return null;
            return _matches[_matches.Count - 1];
        }


        private void ProcessOperatorForGroup()
        {
            if (this.LastOperator() == "(")
            {
                this.RemoveLastOperator();
            }
        }


        private void ProcessOperatorForOr()
        {
            if (this._orCount == 0 || this.LastOperator() != "|")
            {
                return;
            }

            var matches = this.GetMatchCollection();
            if (matches.Count >= 2)
            {
                // 1. Get last 2
                var lastIndex = matches.Count - 1;
                var right = matches[lastIndex];
                var left = matches[lastIndex - 1];

                // 2. Create a or group
                var groupOr = new TokenGroupOr(left, right);

                // 3. Remove last 2
                matches.RemoveAt(lastIndex);
                matches.RemoveAt(lastIndex - 1);
                matches.Add(groupOr);

                _orCount--;
                this.RemoveLastOperator();
            }
        }


        #region Read methods
        /// <summary>
        /// Reads a token e.g. usually begins with "$"
        /// </summary>
        /// <returns></returns>
        private TokenMatch ReadToken()
        {
            var start = _pos;
            var match = new TokenMatch();
            match.IsRequired = true;
            
            while (_pos < END)
            {
                var c = _grammar[_pos];
                // Case 1: Begin a token match.
                if (c == '$' || c == '#')
                {
                    _pos++;
                    var word = ReadWord();
                    match.Name = word;

                    // Expect ":" followed by tokenvalue
                    this.Expect(':');
                    this.ReadTokenValue(match);
                    break;
                }
                // Case 2: Move past white space.
                else if (c == '\t' || c == ' ')
                {
                    _pos++;
                }
                else if ( c == '\'')
                {
                    _pos++;
                    match.Text = ReadString();
                    break;
                }
                else
                    break;
            }
            return match;
        }


        /// <summary>
        /// Reads a token value : usually after ":" and includes type e.g. @number @bool
        /// </summary>
        /// <param name="match"></param>
        private void ReadTokenValue(TokenMatch match)
        {
            var c = _grammar[_pos];
            if (c == '@')
            {
                _pos++;
                var tokenType = ReadWord();
                match.TokenType = "@" + tokenType;

                if (_pos < END)
                {
                    // Check for ".value"
                    c = _grammar[_pos];
                    if (c == '.')
                    {
                        _pos++;
                        var val = ReadWord();
                        match.TokenPropEnabled = true;
                        match.TokenPropValue = val;
                    }
                }
            }
            else if (c == '(')
            {
                _pos++;
                var words = ReadList();
                match.Values = words;
            }
            else if (c == ':' || c == '$')
            {
                _pos++;
                match.Text = c.ToString();
            }
            else if (char.IsLetter(c))
            {
                match.Text = ReadWord();
            }
        }


        /// <summary>
        /// Reads a single word made up of only characters.
        /// </summary>
        /// <returns></returns>
        private string ReadWord()
        {
            var c = _grammar[_pos];
            var start = _pos;
            while (_pos < END && ( Char.IsLetter(c)) || ( _pos > start && Char.IsNumber(c) ))
            {
                _pos++;
                if (_pos >= END)
                    break;
                c = _grammar[_pos]; 
            }
            var text = _grammar.Substring(start, _pos - start);
            return text;
        }


        /// <summary>
        /// Reads a string literal containing within single quotes e.g. ''
        /// </summary>
        /// <returns></returns>
        private string ReadString()
        {
            var c = _grammar[_pos];
            var start = _pos;
            while (_pos < END && c != '\'')
            {
                _pos++;
                c = _grammar[_pos];
            }
            var text = _grammar.Substring(start, _pos - start);
            if (c == '\'')
                _pos++;
            return text;
        }


        private string[] ReadList()
        {
            var words = new List<string>();
            while (_pos < END)
            {
                var c = _grammar[_pos];
                if (c == ')')
                {
                    _pos++;
                    break;
                }
                if (c == '\t' || c == ' ' || c == '|')
                {
                    _pos++;
                }
                else if (Char.IsLetter(c))
                {
                    var word = ReadWord();
                    words.Add(word);
                }
            }
            return words.ToArray();
        }
        #endregion


        private void Expect(char expectChar)
        {
            var c = _grammar[_pos];
            if(c != expectChar)
            {
                this.AddError("Expected : " + expectChar + ", but found : " + c + " at position : " + _pos);
                return;
            }
            _pos++;
        }


        private void AddError(string error)
        {
            this._errors.Add(error);
        }
    }

}
