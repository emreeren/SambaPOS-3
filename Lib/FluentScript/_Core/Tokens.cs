using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// The categories of tokens.
    /// </summary>
    public class TokenKind
    {
        /// <summary>
        /// Keyword token such as var function for while if
        /// </summary>
        public const int Keyword = 0;


        /// <summary>
        /// Symbol such as + - / !=
        /// </summary>
        public const int Symbol = 1;


        /// <summary>
        /// Identifier such as variabls, function names.
        /// </summary>
        public const int Ident = 3;


        /// <summary>
        /// Comment - single line or multiline
        /// </summary>
        public const int Comment = 4;


        /// <summary>
        /// Represents a token that is made up of multiple tokens ( used for interpolated strings )
        /// </summary>
        public const int Multi = 5;


        /// <summary>
        /// Other type of token.
        /// </summary>
        public const int Other = 6;


        /// <summary>
        /// String, bool, numeric literal such as true, 0, false
        /// </summary>
        public const int LiteralString = 20;


        /// <summary>
        /// String, bool, numeric literal such as true, 0, false
        /// </summary>
        public const int LiteralNumber = 21;


        /// <summary>
        /// String, bool, numeric literal such as true, 0, false
        /// </summary>
        public const int LiteralDate = 22;


        /// <summary>
        /// String, bool, numeric literal such as true, 0, false
        /// </summary>
        public const int LiteralBool = 23;


        /// <summary>
        /// String, bool, numeric literal such as true, 0, false
        /// </summary>
        public const int LiteralTime = 24;


        /// <summary>
        /// String, bool, numeric literal such as true, 0, false
        /// </summary>
        public const int LiteralOther = 25;
    }



    /// <summary>
    /// Represents ids for tokens
    /// </summary>
    public class TokenTypes
    {
        // Keywords
        public const int Var = 100;
        public const int If = 101;
        public const int Else = 102;
        public const int Break = 103;
        public const int Continue = 104;
        public const int For = 105;
        public const int While = 106;
        public const int Function = 107;
        public const int Return = 108;
        public const int New = 109;
        public const int Try = 110;
        public const int Catch = 111;
        public const int Throw = 112;
        public const int In = 113;
        public const int Run = 114;
        public const int Then = 115;

        public const int True = 200;
        public const int False = 201;
        public const int Null = 202;
        public const int WhiteSpace = 203;
        public const int NewLine = 204;
        public const int CommentSLine = 205;
        public const int CommentMLine = 206;
        public const int Ident = 207;
        public const int LiteralString = 208;
        public const int LiteralNumber = 209;
        public const int LiteralDate = 210;
        public const int LiteralOther = 211;
        public const int LiteralBool = 212;
        public const int LiteralVersion = 213;

        // Comparision operators ( < <= > >= == != )
        public const int Plus = 300;
        public const int Minus = 301;
        public const int Multiply = 302;
        public const int Divide = 303;
        public const int Modulo = 304;
        public const int LessThan = 305;
        public const int LessThanOrEqual = 306;
        public const int MoreThan = 307;
        public const int MoreThanOrEqual = 308;
        public const int EqualEqual = 309;
        public const int NotEqual = 310;
        public const int LogicalAnd = 311;
        public const int LogicalOr = 312;
        public const int LogicalNot = 313;
        public const int Conditional = 314;
        public const int Increment = 315;
        public const int Decrement = 316;
        public const int IncrementAdd = 317;
        public const int IncrementSubtract = 318;
        public const int IncrementMultiply = 319;
        public const int IncrementDivide = 320;
        public const int LeftBrace = 321;
        public const int RightBrace = 322;
        public const int LeftParenthesis = 323;
        public const int RightParenthesis = 324;
        public const int LeftBracket = 325;
        public const int RightBracket = 326;
        public const int Semicolon = 327;
        public const int Comma = 328;
        public const int Dot = 329;
        public const int Colon = 330;
        public const int Assignment = 331;
        public const int Dollar = 332;
        public const int At = 333;
        public const int Pound = 334;
        public const int Pipe = 335;
        public const int BackSlash = 336;

        public const int EOF = 400;
        public const int Empty = 401;
        public const int Multi = 402;
        public const int Unknown = 403;
    }



    #region Token
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
    #endregion


    /// <summary>
    /// Holds all the tokens in the system.
    /// </summary>
    public class Tokens
    {
        public static readonly Token EndToken = ToKeyword(TokenTypes.EOF, "EOF");
        public static readonly Token Empty = ToKeyword(TokenTypes.Empty, "Empty");
        public static readonly Token Unknown = ToKeyword(TokenTypes.Unknown, "unknown");
        // Keyword tokens
        public static readonly Token Var = ToKeyword(TokenTypes.Var, "var");
        public static readonly Token If = ToKeyword(TokenTypes.If, "if");
        public static readonly Token Else = ToKeyword(TokenTypes.Else, "else");
        public static readonly Token Break = ToKeyword(TokenTypes.Break, "break");
        public static readonly Token Continue = ToKeyword(TokenTypes.Continue, "continue");
        public static readonly Token For = ToKeyword(TokenTypes.For, "for");
        public static readonly Token While = ToKeyword(TokenTypes.While, "while");
        public static readonly Token Function = ToKeyword(TokenTypes.Function, "function");
        public static readonly Token Return = ToKeyword(TokenTypes.Return, "return");
        public static readonly Token New = ToKeyword(TokenTypes.New, "new");
        public static readonly Token Try = ToKeyword(TokenTypes.Try, "try");
        public static readonly Token Catch = ToKeyword(TokenTypes.Catch, "catch");
        public static readonly Token Throw = ToKeyword(TokenTypes.Throw, "throw");
        public static readonly Token In = ToKeyword(TokenTypes.In, "in");
        public static readonly Token Run = ToKeyword(TokenTypes.Run, "run");
        public static readonly Token Then = ToKeyword(TokenTypes.Then, "then");

        // Literal tokens.
        public static readonly Token True = ToLiteralBool(TokenTypes.True, "true", true);
        public static readonly Token False = ToLiteralBool(TokenTypes.False, "false", false);
        public static readonly Token Null = ToLiteralOther(TokenTypes.Null, "null", null);
        public static readonly Token WhiteSpace = ToLiteralOther(TokenTypes.WhiteSpace, " ", string.Empty);
        public static readonly Token NewLine = ToLiteralOther(TokenTypes.NewLine, "newline", string.Empty);
        public static readonly Token CommentSLine = ToLiteralOther(TokenTypes.CommentSLine, "comment_sl", string.Empty);
        public static readonly Token CommentMLine = ToLiteralOther(TokenTypes.CommentMLine, "comment_ml", string.Empty);

        // Symbols ( Math ( + - * / % ), Compare( < <= > >= != == ), Other( [ { } ] ( ) . , ; # $ )
        public static readonly Token Plus = ToSymbol(TokenTypes.Plus, "+");
        public static readonly Token Minus = ToSymbol(TokenTypes.Minus, "-");
        public static readonly Token Multiply = ToSymbol(TokenTypes.Multiply, "*");
        public static readonly Token Divide = ToSymbol(TokenTypes.Divide, "/");
        public static readonly Token Modulo = ToSymbol(TokenTypes.Modulo, "%");
        public static readonly Token LessThan = ToSymbol(TokenTypes.LessThan, "<");
        public static readonly Token LessThanOrEqual = ToSymbol(TokenTypes.LessThanOrEqual, "<=");
        public static readonly Token MoreThan = ToSymbol(TokenTypes.MoreThan, ">");
        public static readonly Token MoreThanOrEqual = ToSymbol(TokenTypes.MoreThanOrEqual, ">=");
        public static readonly Token EqualEqual = ToSymbol(TokenTypes.EqualEqual, "==");
        public static readonly Token NotEqual = ToSymbol(TokenTypes.NotEqual, "!=");
        public static readonly Token LogicalAnd = ToSymbol(TokenTypes.LogicalAnd, "&&");
        public static readonly Token LogicalOr = ToSymbol(TokenTypes.LogicalOr, "||");
        public static readonly Token LogicalNot = ToSymbol(TokenTypes.LogicalNot, "!");
        public static readonly Token Conditional = ToSymbol(TokenTypes.Conditional, "?");
        public static readonly Token Increment = ToSymbol(TokenTypes.Increment, "++");
        public static readonly Token Decrement = ToSymbol(TokenTypes.Decrement, "--");
        public static readonly Token IncrementAdd = ToSymbol(TokenTypes.IncrementAdd, "+=");
        public static readonly Token IncrementSubtract = ToSymbol(TokenTypes.IncrementSubtract, "-=");
        public static readonly Token IncrementMultiply = ToSymbol(TokenTypes.IncrementMultiply, "*=");
        public static readonly Token IncrementDivide = ToSymbol(TokenTypes.IncrementDivide, "/=");
        public static readonly Token LeftBrace = ToSymbol(TokenTypes.LeftBrace, "{");
        public static readonly Token RightBrace = ToSymbol(TokenTypes.RightBrace, "}");
        public static readonly Token LeftParenthesis = ToSymbol(TokenTypes.LeftParenthesis, "(");
        public static readonly Token RightParenthesis = ToSymbol(TokenTypes.RightParenthesis, ")");
        public static readonly Token LeftBracket = ToSymbol(TokenTypes.LeftBracket, "[");
        public static readonly Token RightBracket = ToSymbol(TokenTypes.RightBracket, "]");
        public static readonly Token Semicolon = ToSymbol(TokenTypes.Semicolon, ";");
        public static readonly Token Comma = ToSymbol(TokenTypes.Comma, ",");
        public static readonly Token Dot = ToSymbol(TokenTypes.Dot, ".");
        public static readonly Token Colon = ToSymbol(TokenTypes.Colon, ":");
        public static readonly Token Assignment = ToSymbol(TokenTypes.Assignment, "=");
        public static readonly Token Dollar = ToSymbol(TokenTypes.Dollar, "$");
        public static readonly Token At = ToSymbol(TokenTypes.At, "@");
        public static readonly Token Pound = ToSymbol(TokenTypes.Pound, "#");
        public static readonly Token Pipe = ToSymbol(TokenTypes.Pipe, "|");
        public static readonly Token BackSlash = ToSymbol(TokenTypes.BackSlash, "\\");


        internal static IDictionary<string, Token> AllTokens = new Dictionary<string, Token>();
        internal static IDictionary<string, Token> CompareTokens = new Dictionary<string, Token>();
        internal static IDictionary<string, Token> MathTokens = new Dictionary<string, Token>();
        internal static IDictionary<string, bool> ExpressionCombinatorOps = new Dictionary<string, bool>()
        {
            { "*",       true },
            { "/",       true },
            { "%",       true },
            { "+",       true },
            { "-",       true },
            { "<",       true },
            { "<=",      true },
            { ">",       true },
            { ">=",      true },
            { "!=",      true },
            { "==",      true },
            { "&&",      true },
            { "||",      true },
            { "]",       true },
            { ")",       true },
            { "}",       true },
        };


        /// <summary>
        /// Defaults the token collection.
        /// </summary>
        public static void Default()
        {
            // NOTE: May not need these mappings.
            // But leaving it here until refactoring is done.
            AddToLookup(Var);
            AddToLookup(If);
            AddToLookup(Else);
            AddToLookup(For);
            AddToLookup(While);
            AddToLookup(Function);
            AddToLookup(Break);
            AddToLookup(Continue);
            AddToLookup(New);
            AddToLookup(Return);
            AddToLookup(Try);
            AddToLookup(Catch);
            AddToLookup(Throw);
            AddToLookup(In);
            AddToLookup(Run);
            AddToLookup(Then);

            AddToLookup(Plus);
            AddToLookup(Minus);
            AddToLookup(Multiply);
            AddToLookup(Divide);
            AddToLookup(Modulo);
            AddToLookup(LessThan);
            AddToLookup(LessThanOrEqual);
            AddToLookup(MoreThan);
            AddToLookup(MoreThanOrEqual);
            AddToLookup(EqualEqual);
            AddToLookup(NotEqual);
            AddToLookup(LogicalAnd);
            AddToLookup(LogicalOr);
            AddToLookup(LogicalNot);
            AddToLookup(Conditional);
            AddToLookup(Increment);
            AddToLookup(Decrement);
            AddToLookup(IncrementAdd);
            AddToLookup(IncrementSubtract);
            AddToLookup(IncrementMultiply);
            AddToLookup(IncrementDivide);
            AddToLookup(LeftBrace);
            AddToLookup(RightBrace);
            AddToLookup(LeftParenthesis);
            AddToLookup(RightParenthesis);
            AddToLookup(LeftBracket);
            AddToLookup(RightBracket);
            AddToLookup(Semicolon);
            AddToLookup(Comma);
            AddToLookup(Dot);
            AddToLookup(Colon);
            AddToLookup(Assignment);
            AddToLookup(Dollar);
            AddToLookup(At);
            AddToLookup(Pound);
            AddToLookup(Pipe);
            AddToLookup(BackSlash);

            AddToLookup(True);
            AddToLookup(False);
            AddToLookup(Null);
            AddToLookup(WhiteSpace);
            AddToLookup(NewLine);
            AddToLookup(CommentSLine);
            AddToLookup(CommentMLine);

            RegisterCompareOps("<", "<=", ">", ">=", "!=", "==");
            RegisterMathOps("*", "/", "+", "-", "%");
        }


        /// <summary>
        /// Determines if the text supplied is a literal token
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsLiteral(string text)
        {
            bool contains = AllTokens.ContainsKey(text);
            if (!contains) return false;
            Token t = AllTokens[text];
            return t.Kind >= TokenKind.LiteralString;
        }


        /// <summary>
        /// Gets whether or not the text provided is a keyword
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsKeyword(string text)
        {
            return IsTokenKind(text, TokenKind.Keyword);
        }


        /// <summary>
        /// Gets whether or not the key provided is a symbol
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsSymbol(string text)
        {
            return IsTokenKind(text, TokenKind.Symbol);
        }


        /// <summary>
        /// Checks if the token is a comparison token ( less lessthan more morethan equal not equal ).
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsCompare(Token token)
        {
            return CompareTokens.ContainsKey(token.Text);
        }


        /// <summary>
        /// Checks if the token supplied is a math op ( * / + - % )
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsMath(Token token)
        {
            return MathTokens.ContainsKey(token.Text);
        }


        /// <summary>
        /// Determines if the text supplied is a literal token
        /// </summary>
        /// <param name="key"></param>
        /// <param name="tokenKind">The TokenKind</param>
        /// <returns></returns>
        public static bool IsTokenKind(string key, int tokenKind)
        {
            bool contains = AllTokens.ContainsKey(key);
            if (!contains) return false;
            Token t = AllTokens[key];
            return t.Kind == tokenKind;
        }


        /// <summary>
        /// Adds the token to the lookup
        /// </summary>
        /// <param name="token"></param>
        public static void AddToLookup(Token token)
        {
            AllTokens[token.Text] = token;
        }


        /// <summary>
        /// Registers compares operators.
        /// </summary>
        /// <param name="ops"></param>
        public static void RegisterCompareOps(params string[] ops)
        {
            foreach (string op in ops)
            {
                var tokenOp = AllTokens[op];
                CompareTokens[op] = tokenOp;
            }
        }


        /// <summary>
        /// Registers math ops.
        /// </summary>
        /// <param name="ops"></param>
        public static void RegisterMathOps(params string[] ops)
        {
            foreach (string op in ops)
            {
                var tokenOp = AllTokens[op];
                MathTokens[op] = tokenOp;
            }
        }


        /// <summary>
        /// Creates a keyword token from the information supplied
        /// </summary>
        /// <param name="typeVal">The numeric value identifying the token</param>
        /// <param name="text">The text representing the token</param>
        /// <returns></returns>
        public static Token ToKeyword(int typeVal, string text)
        {
            return new Token(TokenKind.Keyword, typeVal, text, text);
        }


        /// <summary>
        /// Creates a comment token from the information supplied
        /// </summary>
        /// <param name="isMultiline">Whether or not the text is a multi-line token</param>
        /// <param name="text">The comment text</param>
        /// <returns></returns>
        public static Token ToComment(bool isMultiline, string text)
        {
            int val = isMultiline ? TokenTypes.CommentMLine : TokenTypes.CommentSLine;
            return new Token(TokenKind.Comment, val, text, text);
        }


        /// <summary>
        /// Creates a symbol token from the information supplied
        /// </summary>
        /// <param name="typeVal">The numeric value identifying the token</param>
        /// <param name="text">The text representing the token</param>
        /// <returns></returns>
        public static Token ToSymbol(int typeVal, string text)
        {
            return new Token(TokenKind.Symbol, typeVal, text, text);
        }


        /// <summary>
        /// Creates a string literal token from the information supplied
        /// </summary>
        /// <param name="text">The text representing the token</param>
        /// <returns></returns>
        public static Token ToLiteralString(string text)
        {
            return new Token(TokenKind.LiteralString, TokenTypes.LiteralString, text, text);
        }


        /// <summary>
        /// Creates a bool literal token from the information supplied
        /// </summary>
        /// <param name="text">The text representing the token</param>
        /// <returns></returns>
        public static Token ToLiteralBool(int typeVal, string text, object val)
        {
            return new Token(TokenKind.LiteralBool, typeVal, text, val);
        }


        /// <summary>
        /// Creates a string literal token from the information supplied
        /// </summary>
        /// <param name="text">The text representing the number</param>
        /// <returns></returns>
        public static Token ToLiteralNumber(string text)
        {
            var val = Convert.ToDouble(text);
            return new Token(TokenKind.LiteralNumber, TokenTypes.LiteralNumber, text, val);
        }


        /// <summary>
        /// Creates a date literal token from the information supplied
        /// </summary>
        /// <param name="text">The text representing the date</param>
        /// <returns></returns>
        public static Token ToLiteralDate(string text)
        {
            var val = DateTime.Parse(text);
            return new Token(TokenKind.LiteralDate, TokenTypes.LiteralDate, text, val);
        }


        /// <summary>
        /// Creates a version literal token from the information supplied
        /// </summary>
        /// <param name="text">The text representing the version</param>
        /// <returns></returns>
        public static Token ToLiteralVersion(string text)
        {
            var val = Version.Parse(text);
            var lv = new LVersion(val);
            return new Token(TokenKind.LiteralOther, TokenTypes.LiteralVersion, text, lv);
        }


        /// <summary>
        /// Creates a string literal token from the information supplied
        /// </summary>
        /// <param name="text">The text representing the token</param>
        /// <returns></returns>
        public static Token ToLiteralOther(int typeVal, string text, object val)
        {
            return new Token(TokenKind.LiteralOther, typeVal, text, text);
        }


        /// <summary>
        /// Creates an identifier token from the information supplied
        /// </summary>
        /// <param name="text">The text representing the token</param>
        /// <returns></returns>
        public static Token ToIdentifier(string text)
        {
            return new Token(TokenKind.Ident, TokenTypes.Ident, text, text);
        }


        /// <summary>
        /// Creates an interpolated token from the information supplied
        /// </summary>
        /// <param name="text">The text representing the token</param>
        /// <returns></returns>
        public static Token ToInterpolated(string text, List<TokenData> tokens)
        {
            return new Token(TokenKind.Multi, TokenTypes.Multi, text, tokens);
        }


        /// <summary>
        /// Creates a token from an existing token and position information.
        /// </summary>
        /// <param name="token">The token to copy from.</param>
        /// <param name="line">The line number</param>
        /// <param name="lineCharPos">The line char position</param>
        /// <param name="charPos">The char position</param>
        /// <returns></returns>
        public static Token ToToken(Token token, int line, int lineCharPos, int charPos)
        {
            var t = new Token(token.Kind, token.Type, token.Text, token.Value);
            //{ Line = line, LineCharPos = lineCharPos, Pos = charPos };
            return t;
        }


        /// <summary>
        /// Creates a token from an existing token and position information.
        /// </summary>
        /// <param name="text">The text associated with the token to clone.</param>
        /// <returns></returns>
        public static Token Clone(string text)
        {
            var t = AllTokens[text];
            return t.Clone();
        }


        /// <summary>
        /// Returns a token by looking it up by it's text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Token Lookup(string text)
        {
            return AllTokens[text];
        }
    }
}