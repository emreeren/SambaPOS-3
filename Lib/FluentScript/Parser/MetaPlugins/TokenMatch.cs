using System;
using System.Collections.Generic;
using System.Linq;
using ComLib.Lang.Core;
using ComLib.Lang.Parsing;


namespace ComLib.Lang.Parsing.MetaPlugins
{

    // Used to represent a token match for the grammer check in a plugin.
    public class TokenMatch
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="varname"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="required"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="vals"></param>
        public TokenMatch Init(string varname, int tokenIndex, bool required, string name, string type, int min, int max, string[] vals)
        {
            this.VarName = varname;
            this.TokenIndex = tokenIndex;
            this.IsRequired = required;
            this.Name = name;
            this.TokenType = type;
            this.Min = min;
            this.Max = max;
            this.Values = vals;
            return this;
        }


        // Named token. can be @starttoken, @n1, @n2, @n3
        public string Name;


        // Name of the variable that should be used to store this value. ( used in parsing )
        public string VarName;


        // Either 0 representing @starttoken, 1 = n1, 2 = n2, 3 = n3
        public int TokenIndex;


        // The token type ( e.g. ) number, ident, bool, etc.
        public string TokenType;
        

        // Whether or not this is required
        public bool IsRequired;


        // Whether or not there is a range check
        public bool HasRange;


        /// <summary>
        /// Whether or not to use a start token value instead of it's key.
        /// e.g. for days "monday" : 1 , this indicates using "1" instead of "monday"
        /// </summary>
        public bool TokenPropEnabled;


        /// <summary>
        /// A token property value.
        /// </summary>
        public string TokenPropValue;


        /// <summary>
        /// Whether or not this a group.
        /// </summary>
        public bool IsGroup;


        /// <summary>
        /// The text to match.
        /// </summary>
        public string Text;


        /// <summary>
        /// Optional values to match against
        /// </summary>
        public string[] Values;


        // 1 as in {1, 2}
        public int Min;

        // 2 as in {1, 2}
        public int Max;


        public bool IsMatchingType(Token token)
        {
            if (string.IsNullOrEmpty(this.TokenType))
                return false;
            if (this.TokenType == "@number" && token.Kind == TokenKind.LiteralNumber)
                return true;
            if (this.TokenType == "@time" && token.Kind == TokenKind.LiteralTime)
                return true; 
            if (this.TokenType == "@word" && token.Kind == TokenKind.Ident)
                return true;
            return false;
        }


        public bool IsMatchingValue(Token token)
        {
            if (this.Values == null || this.Values.Length == 0)
                return true;
            return this.Values.Contains(token.Text);
        }
    }
}
