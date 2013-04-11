#pragma warning disable 1591
using System.Collections.Generic;
using Fluentscript.Lib.Helpers;

namespace Fluentscript.Lib._Core
{
    /// ------------------------------------------------------------------------------------------------
    /// remarks: This file is auto-generated from the FSGrammar specification and should not be modified.
    /// summary: This file contains all the AST for expressions at the system level.
    ///			features like control-flow e..g if, while, for, try, break, continue, return etc.
    /// version: 0.9.8.10
    /// author:  kishore reddy
    /// date:	01/18/13 02:48:07 PM
    /// ------------------------------------------------------------------------------------------------

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


    public class TokenTypes
    {
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

        public const int Plus = 300;
        public const int Minus = 301;
        public const int Multiply = 302;
        public const int Divide = 303;
        public const int Percent = 304;
        public const int LessThan = 305;
        public const int LessThanOrEqual = 306;
        public const int MoreThan = 307;
        public const int MoreThanOrEqual = 308;
        public const int EqualEqual = 309;
        public const int NotEqual = 310;
        public const int LogicalAnd = 311;
        public const int LogicalOr = 312;
        public const int LogicalNot = 313;
        public const int Question = 314;
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

        public const int EndToken = 400;
        public const int Unknown = 401;
        public const int Multi = 402;

        public const int True = 200;
        public const int False = 201;
        public const int Null = 202;
        public const int WhiteSpace = 203;
        public const int NewLine = 204;
        public const int CommentSLine = 205;
        public const int CommentMLine = 206;
        public const int Ident = 207;
        public const int LiteralBool = 208;
        public const int LiteralDate = 209;
        public const int LiteralDay = 210;
        public const int LiteralNumber = 211;
        public const int LiteralString = 212;
        public const int LiteralTime = 213;
        public const int LiteralVersion = 214;
        public const int LiteralOther = 215;
    }


    public class Tokens
    {
        public static readonly Token Var = TokenBuilder.ToKeyword(TokenTypes.Var, "var");
        public static readonly Token If = TokenBuilder.ToKeyword(TokenTypes.If, "if");
        public static readonly Token Else = TokenBuilder.ToKeyword(TokenTypes.Else, "else");
        public static readonly Token Break = TokenBuilder.ToKeyword(TokenTypes.Break, "break");
        public static readonly Token Continue = TokenBuilder.ToKeyword(TokenTypes.Continue, "continue");
        public static readonly Token For = TokenBuilder.ToKeyword(TokenTypes.For, "for");
        public static readonly Token While = TokenBuilder.ToKeyword(TokenTypes.While, "while");
        public static readonly Token Function = TokenBuilder.ToKeyword(TokenTypes.Function, "function");
        public static readonly Token Return = TokenBuilder.ToKeyword(TokenTypes.Return, "return");
        public static readonly Token New = TokenBuilder.ToKeyword(TokenTypes.New, "new");
        public static readonly Token Try = TokenBuilder.ToKeyword(TokenTypes.Try, "try");
        public static readonly Token Catch = TokenBuilder.ToKeyword(TokenTypes.Catch, "catch");
        public static readonly Token Throw = TokenBuilder.ToKeyword(TokenTypes.Throw, "throw");
        public static readonly Token In = TokenBuilder.ToKeyword(TokenTypes.In, "in");
        public static readonly Token Run = TokenBuilder.ToKeyword(TokenTypes.Run, "run");
        public static readonly Token Then = TokenBuilder.ToKeyword(TokenTypes.Then, "then");
        public static readonly Token Plus = TokenBuilder.ToSymbol(TokenTypes.Plus, "+");

        public static readonly Token Minus = TokenBuilder.ToSymbol(TokenTypes.Minus, "-");
        public static readonly Token Multiply = TokenBuilder.ToSymbol(TokenTypes.Multiply, "*");
        public static readonly Token Divide = TokenBuilder.ToSymbol(TokenTypes.Divide, "/");
        public static readonly Token Percent = TokenBuilder.ToSymbol(TokenTypes.Percent, "%");
        public static readonly Token LessThan = TokenBuilder.ToSymbol(TokenTypes.LessThan, "<");
        public static readonly Token LessThanOrEqual = TokenBuilder.ToSymbol(TokenTypes.LessThanOrEqual, "<=");
        public static readonly Token MoreThan = TokenBuilder.ToSymbol(TokenTypes.MoreThan, ">");
        public static readonly Token MoreThanOrEqual = TokenBuilder.ToSymbol(TokenTypes.MoreThanOrEqual, ">=");
        public static readonly Token EqualEqual = TokenBuilder.ToSymbol(TokenTypes.EqualEqual, "==");
        public static readonly Token NotEqual = TokenBuilder.ToSymbol(TokenTypes.NotEqual, "!=");
        public static readonly Token LogicalAnd = TokenBuilder.ToSymbol(TokenTypes.LogicalAnd, "&&");
        public static readonly Token LogicalOr = TokenBuilder.ToSymbol(TokenTypes.LogicalOr, "||");
        public static readonly Token LogicalNot = TokenBuilder.ToSymbol(TokenTypes.LogicalNot, "!");
        public static readonly Token Question = TokenBuilder.ToSymbol(TokenTypes.Question, "?");
        public static readonly Token Increment = TokenBuilder.ToSymbol(TokenTypes.Increment, "++");
        public static readonly Token Decrement = TokenBuilder.ToSymbol(TokenTypes.Decrement, "--");
        public static readonly Token IncrementAdd = TokenBuilder.ToSymbol(TokenTypes.IncrementAdd, "+=");
        public static readonly Token IncrementSubtract = TokenBuilder.ToSymbol(TokenTypes.IncrementSubtract, "-=");
        public static readonly Token IncrementMultiply = TokenBuilder.ToSymbol(TokenTypes.IncrementMultiply, "*=");
        public static readonly Token IncrementDivide = TokenBuilder.ToSymbol(TokenTypes.IncrementDivide, "/=");
        public static readonly Token LeftBrace = TokenBuilder.ToSymbol(TokenTypes.LeftBrace, "{");
        public static readonly Token RightBrace = TokenBuilder.ToSymbol(TokenTypes.RightBrace, "}");
        public static readonly Token LeftParenthesis = TokenBuilder.ToSymbol(TokenTypes.LeftParenthesis, "(");
        public static readonly Token RightParenthesis = TokenBuilder.ToSymbol(TokenTypes.RightParenthesis, ")");
        public static readonly Token LeftBracket = TokenBuilder.ToSymbol(TokenTypes.LeftBracket, "[");
        public static readonly Token RightBracket = TokenBuilder.ToSymbol(TokenTypes.RightBracket, "]");
        public static readonly Token Semicolon = TokenBuilder.ToSymbol(TokenTypes.Semicolon, ";");
        public static readonly Token Comma = TokenBuilder.ToSymbol(TokenTypes.Comma, ",");
        public static readonly Token Dot = TokenBuilder.ToSymbol(TokenTypes.Dot, ".");
        public static readonly Token Colon = TokenBuilder.ToSymbol(TokenTypes.Colon, ":");
        public static readonly Token Assignment = TokenBuilder.ToSymbol(TokenTypes.Assignment, "=");
        public static readonly Token Dollar = TokenBuilder.ToSymbol(TokenTypes.Dollar, "$");
        public static readonly Token At = TokenBuilder.ToSymbol(TokenTypes.At, "@");
        public static readonly Token Pound = TokenBuilder.ToSymbol(TokenTypes.Pound, "#");
        public static readonly Token Pipe = TokenBuilder.ToSymbol(TokenTypes.Pipe, "|");
        public static readonly Token BackSlash = TokenBuilder.ToSymbol(TokenTypes.BackSlash, "\\");

        public static readonly Token EndToken = TokenBuilder.ToLiteralOther(TokenTypes.EndToken, "eof", "eof");
        public static readonly Token Unknown = TokenBuilder.ToLiteralOther(TokenTypes.Unknown, "unknown", "unknown");
        public static readonly Token Ignore = TokenBuilder.ToLiteralOther(TokenTypes.Unknown, "ignore", "ignore");

        public static readonly Token True = TokenBuilder.ToLiteralBool(TokenTypes.True, "true", true);
        public static readonly Token False = TokenBuilder.ToLiteralBool(TokenTypes.False, "false", false);
        public static readonly Token Null = TokenBuilder.ToLiteralOther(TokenTypes.Null, "null", null);
        public static readonly Token WhiteSpace = TokenBuilder.ToLiteralOther(TokenTypes.WhiteSpace, " ", "");
        public static readonly Token NewLine = TokenBuilder.ToLiteralOther(TokenTypes.NewLine, "newline", "newline");
        public static readonly Token CommentSLine = TokenBuilder.ToLiteralOther(TokenTypes.CommentSLine, "comment_sl", "comment_sl");
        public static readonly Token CommentMLine = TokenBuilder.ToLiteralOther(TokenTypes.CommentMLine, "comment_ml", "comment_ml");	

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
            AddToLookup(Tokens.Var);
            AddToLookup(Tokens.If);
            AddToLookup(Tokens.Else);
            AddToLookup(Tokens.For);
            AddToLookup(Tokens.While);
            AddToLookup(Tokens.Function);
            AddToLookup(Tokens.Break);
            AddToLookup(Tokens.Continue);
            AddToLookup(Tokens.New);
            AddToLookup(Tokens.Return);
            AddToLookup(Tokens.Try);
            AddToLookup(Tokens.Catch);
            AddToLookup(Tokens.Throw);
            AddToLookup(Tokens.In);
            AddToLookup(Tokens.Run);
            AddToLookup(Tokens.Then);

            AddToLookup(Tokens.Plus);
            AddToLookup(Tokens.Minus);
            AddToLookup(Tokens.Multiply);
            AddToLookup(Tokens.Divide);
            AddToLookup(Tokens.Percent);
            AddToLookup(Tokens.LessThan);
            AddToLookup(Tokens.LessThanOrEqual);
            AddToLookup(Tokens.MoreThan);
            AddToLookup(Tokens.MoreThanOrEqual);
            AddToLookup(Tokens.EqualEqual);
            AddToLookup(Tokens.NotEqual);
            AddToLookup(Tokens.LogicalAnd);
            AddToLookup(Tokens.LogicalOr);
            AddToLookup(Tokens.LogicalNot);
            AddToLookup(Tokens.Question);
            AddToLookup(Tokens.Increment);
            AddToLookup(Tokens.Decrement);
            AddToLookup(Tokens.IncrementAdd);
            AddToLookup(Tokens.IncrementSubtract);
            AddToLookup(Tokens.IncrementMultiply);
            AddToLookup(Tokens.IncrementDivide);
            AddToLookup(Tokens.LeftBrace);
            AddToLookup(Tokens.RightBrace);
            AddToLookup(Tokens.LeftParenthesis);
            AddToLookup(Tokens.RightParenthesis);
            AddToLookup(Tokens.LeftBracket);
            AddToLookup(Tokens.RightBracket);
            AddToLookup(Tokens.Semicolon);
            AddToLookup(Tokens.Comma);
            AddToLookup(Tokens.Dot);
            AddToLookup(Tokens.Colon);
            AddToLookup(Tokens.Assignment);
            AddToLookup(Tokens.Dollar);
            AddToLookup(Tokens.At);
            AddToLookup(Tokens.Pound);
            AddToLookup(Tokens.Pipe);
            AddToLookup(Tokens.BackSlash);

            AddToLookup(Tokens.True);
            AddToLookup(Tokens.False);
            AddToLookup(Tokens.Null);
            AddToLookup(Tokens.WhiteSpace);
            AddToLookup(Tokens.NewLine);
            AddToLookup(Tokens.CommentSLine);
            AddToLookup(Tokens.CommentMLine);

            RegisterCompareOps("<", "<=", ">", ">=", "!=", "==");
            RegisterMathOps("*", "/", "+", "-", "%");
        }


        public static bool ContainsKey(string text)
        {
            return AllTokens.ContainsKey(text);
        }


        public static Token GetToken(string text)
        {
            return AllTokens[text];
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