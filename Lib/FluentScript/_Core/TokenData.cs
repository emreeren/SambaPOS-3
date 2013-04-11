namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// Wraps a token with contextual information about it's script location.
    /// </summary>
    public class TokenData
    {
        /// <summary>
        /// Index position of the token in the script.
        /// </summary>
        protected int _index;


        /// <summary>
        /// The token
        /// </summary>
        public Token Token;


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


        /// <summary>
        /// Sets the index of this token.
        /// </summary>
        /// <param name="ndx"></param>
        internal void SetIndex(int ndx)
        {
            _index = ndx;
        }


        /// <summary>
        /// String representation of tokendata.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {   
            string tokenType = Token.GetType().Name.Replace("Token", "");
            string text = Token.Text;
            if (Token.Kind == TokenKind.LiteralString)
                text = "'" + text + "'";
            string info = string.Format("Index: {0}, Line: {1}, CharPos: {2}, Pos: {3}, Type: {4}, Text: {5}", Index, Line, LineCharPos, Pos, tokenType, text);
            return info;
        }
    }
}
