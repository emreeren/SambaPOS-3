using System;
using System.Collections.Generic;


namespace ComLib.Lang.Parsing.MetaPlugins
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
        private int _groupCount;


        public List<TokenMatch> Parse(string grammar)
        {
            // $month:@starttoken.value $day:@number $dayend:( st | nd | rd | th )? ( ','?  $year:@number )?"	            
            _pos = 0;
            _grammar = grammar;
            _errors = new List<string>();
            END = _grammar.Length;
            _matches = new List<TokenMatch>();
            while (_pos < END)
            {
                var c = _grammar[_pos];

                // Case 1: Begin a token match.
                if (c == '$' || c == '\'')
                {
                    var match = ReadToken();
                    AddMatch(match);
                }
                // Case 2: Move past white space.
                else if (c == '\t' || c == ' ')
                {
                    _pos++;
                }
                // Case 3: Last token was optional
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
                // Case 4: "(" starts a grouping of token matches
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
                // Case 5: word
                else if (char.IsLetter(c))
                {
                    var word = ReadWord();
                    var match = new TokenMatch();
                    match.Text = word;
                    match.IsRequired = true;
                    AddMatch(match);
                }
                else
                    _pos++;
            }
            return _matches;
        }


        private void BeginGroup()
        {
            _matches.Add(new TokenGroup());
            _groupCount++;
        }

        
        private void CloseGroup()
        {
            _groupCount--;
        }


        private void AddMatch(TokenMatch match)
        {
            var last = Last();
            if(_groupCount > 0 && last != null && last.IsGroup)
            {
                ((TokenGroup) last).Matches.Add(match);
            }
            else
            {
                _matches.Add(match);
            }
        }

        private TokenMatch Last()
        {
            if (_matches.Count == 0) return null;
            return _matches[_matches.Count - 1];
        }


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
                if (c == '$')
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
                        if (val == "value")
                        {
                            match.TokenPropEnabled = true;
                            match.TokenPropValue = val;
                        }
                    }
                }
            }
            else if (c == '(')
            {
                _pos++;
                var words = ReadList();
                match.Values = words;
            }
            else if (c == ':')
            {
                _pos++;
                match.Text = ":";
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
            while (Char.IsLetter(c) && _pos < END)
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
