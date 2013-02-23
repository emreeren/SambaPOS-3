#pragma warning disable 1591
using System;


namespace ComLib.Lang.AST
{
    /// <summary>
    /// Represents a a single character or a series of characters that represents a recogized part of a language. 
    /// e.g. "var" "+" "=" "for" "function"
    /// </summary>
    public class Token
    {
        #region Protected Members
        /// <summary>
        /// Category of the token.
        /// </summary>
        protected int _kind;


        /// <summary>
        /// The type of the token.
        /// </summary>
        protected int _type;


        /// <summary>
        /// Text of the token
        /// </summary>
        protected string _text = string.Empty;


        /// <summary>
        /// The tokens value.
        /// </summary>
        protected object _value = string.Empty;


        /// <summary>
        /// Whether or not this a keyword.
        /// </summary>
        protected bool _isKeyword;

        /// <summary>
        /// Index position of the token in the script.
        /// </summary>
        protected int _index;
        #endregion
        

        #region Constructors
        /// <summary>
        /// Initialize token information
        /// </summary>
        /// <param name="kind">TokenKind.Keyword, TokenKind.Symbol, etc.</param>
        /// <param name="type">TokenTypes.Var, etc.</param>
        /// <param name="text">"var"</param>
        /// <param name="val">"var"</param>
        public Token(int kind, int type, string text, object val)
        {
            _kind = kind;
            _type = type;
            _text = text;
            _value = val;
        }
        #endregion


        #region Value Info
        /// <summary>
        /// The type of the token.
        /// </summary>
        public virtual int Type { get { return _type; } } 


        /// <summary>
        /// The category of the token.
        /// </summary>
        public virtual int Kind { get { return _kind; } } 


        /// <summary>
        /// Text of the token
        /// </summary>
        public virtual string Text { get { return _text; } }


        /// <summary>
        /// Value of the token.
        /// </summary>
        public virtual object Value { get { return _value; } }
        #endregion

        
        #region Position Info
        /// <summary>
        /// Line number of the token
        /// </summary>
        public int Line { get; set; }


        /// <summary>
        /// Char position in the line of the token.
        /// </summary>
        public int LineCharPos { get; set; }


        /// <summary>
        /// The position of the first char of token based on entire script.
        /// </summary>
        public int Pos { get; set; }


        /// <summary>
        /// The index position of the token.
        /// </summary>
        public int Index
        {
            get { return _index; }
        }
        #endregion


        #region Instance Methods
        /// <summary>
        /// Whether or not this token is a keyword
        /// </summary>
        /// <returns></returns>
        public bool IsKeyword() { return _kind == TokenKind.Keyword; }


        /// <summary>
        /// Whether or not this token is a comment
        /// </summary>
        /// <returns></returns>
        public bool IsComment() { return _kind == TokenKind.Comment; }


        /// <summary>
        /// Whether or not this token is a identifier token
        /// </summary>
        /// <returns></returns>
        public bool IsIdent() { return _kind == TokenKind.Ident; }


        /// <summary>
        /// Whether or not this token is a multi-token token
        /// </summary>
        /// <returns></returns>
        public bool IsMulti() { return _kind == TokenKind.Multi; }


        /// <summary>
        /// Whether or not this token is a symbol token
        /// </summary>
        /// <returns></returns>
        public bool IsSymbol() { return _kind == TokenKind.Symbol; }


        /// <summary>
        /// Whether or not this token is a literal token.
        /// </summary>
        /// <returns></returns>
        public bool IsLiteralAny() { return _kind >= TokenKind.LiteralString; }


        /// <summary>
        /// Whether or not this token is a literal token.
        /// </summary>
        /// <returns></returns>
        public bool IsLiteralString() { return _kind >= TokenKind.LiteralString; }


        /// <summary>
        /// Whether or not this token is a literal token.
        /// </summary>
        /// <returns></returns>
        public bool IsLiteralNumber() { return _kind >= TokenKind.LiteralNumber; }


        /// <summary>
        /// Whether or not this token is a literal token.
        /// </summary>
        /// <returns></returns>
        public bool IsLiteralDate() { return _kind >= TokenKind.LiteralDate; }


        /// <summary>
        /// Sets the index of this token.
        /// </summary>
        /// <param name="ndx"></param>
        internal void SetIndex(int ndx)
        {
            _index = ndx;
        }


        /// <summary>
        /// Sets values from another token.
        /// </summary>
        /// <param name="t"></param>
        internal void SetValues(Token t)
        {
            _kind = t._kind;
            _type = t._type;
        }


        /// <summary>
        /// Set the text of the interpolated token.
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            _text = text;
        }


        /// <summary>
        /// Set the text of the interpolated token.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="val"></param>
        public void SetTextAndValue(string text, object val)
        {
            _text = text;
            _value = val;
        }


        /// <summary>
        /// Whether or not the token supplied is a new line.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsNewLine(Token token)
        {
            return (token._type == TokenTypes.NewLine || token._type == TokenTypes.NewLine);
        }


        /// <summary>
        /// String representation of tokendata.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string info = string.Format("Kind: {0}, Type: {1}, Text: {2}, Val: {3}", Kind, Type, Text, Value);
            return info;
        }


        /// <summary>
        /// Clones this instance of the token and returns a new instance with the same values.
        /// </summary>
        /// <returns></returns>
        public Token Clone()
        {
            var token = new Token(this.Kind, this.Type, this.Text, this.Value);
            return token;
        }
        #endregion
    }
}