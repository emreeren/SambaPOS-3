#pragma warning disable 1591
using System;
using System.Collections.Generic;


namespace ComLib.Lang.AST
{
    /// <summary>
    /// Holds all the tokens in the system.
    /// </summary>
    public class Tokens
    {        
        public static readonly Token EndToken           = TokenBuilder.ToKeyword(TokenTypes.EOF              ,    "EOF"      );
        public static readonly Token Unknown            = TokenBuilder.ToKeyword(TokenTypes.Unknown          ,    "unknown"  );
        
        // Keyword tokens
        public static readonly Token Var                = TokenBuilder.ToKeyword(TokenTypes.Var              ,    "var"      );
        public static readonly Token If                 = TokenBuilder.ToKeyword(TokenTypes.If               ,    "if"       );
        public static readonly Token Else               = TokenBuilder.ToKeyword(TokenTypes.Else             ,    "else"     );
        public static readonly Token Break              = TokenBuilder.ToKeyword(TokenTypes.Break            ,    "break"    );
        public static readonly Token Continue           = TokenBuilder.ToKeyword(TokenTypes.Continue         ,    "continue" );
        public static readonly Token For                = TokenBuilder.ToKeyword(TokenTypes.For              ,    "for"      );
        public static readonly Token While              = TokenBuilder.ToKeyword(TokenTypes.While            ,    "while"    );
        public static readonly Token Function           = TokenBuilder.ToKeyword(TokenTypes.Function         ,    "function" );
        public static readonly Token Return             = TokenBuilder.ToKeyword(TokenTypes.Return           ,    "return"   );
        public static readonly Token New                = TokenBuilder.ToKeyword(TokenTypes.New              ,    "new"      );
        public static readonly Token Try                = TokenBuilder.ToKeyword(TokenTypes.Try              ,    "try"      );
        public static readonly Token Catch              = TokenBuilder.ToKeyword(TokenTypes.Catch            ,    "catch"    );
        public static readonly Token Throw              = TokenBuilder.ToKeyword(TokenTypes.Throw            ,    "throw"    );
        public static readonly Token In                 = TokenBuilder.ToKeyword(TokenTypes.In               ,    "in"       );
        public static readonly Token Run                = TokenBuilder.ToKeyword(TokenTypes.Run              ,    "run"      );
        public static readonly Token Then               = TokenBuilder.ToKeyword(TokenTypes.Then             ,    "then"     );

        // Literal tokens.
        public static readonly Token True               = TokenBuilder.ToLiteralBool(TokenTypes.True         ,    "true",        true         );
        public static readonly Token False              = TokenBuilder.ToLiteralBool(TokenTypes.False        ,    "false",       false        );
        public static readonly Token Null               = TokenBuilder.ToLiteralOther(TokenTypes.Null        ,    "null",        null         );
        public static readonly Token WhiteSpace         = TokenBuilder.ToLiteralOther(TokenTypes.WhiteSpace  ,    " ",           string.Empty );
        public static readonly Token NewLine            = TokenBuilder.ToLiteralOther(TokenTypes.NewLine     ,    "newline",     string.Empty );
        public static readonly Token CommentSLine       = TokenBuilder.ToLiteralOther(TokenTypes.CommentSLine,    "comment_sl",  string.Empty );
        public static readonly Token CommentMLine       = TokenBuilder.ToLiteralOther(TokenTypes.CommentMLine,    "comment_ml",  string.Empty );

        // Symbols ( Math ( + - * / % ), Compare( < <= > >= != == ), Other( [ { } ] ( ) . , ; # $ )
        public static readonly Token Plus               = TokenBuilder.ToSymbol(TokenTypes.Plus              ,    "+"         );
        public static readonly Token Minus              = TokenBuilder.ToSymbol(TokenTypes.Minus             ,    "-"         );
        public static readonly Token Multiply           = TokenBuilder.ToSymbol(TokenTypes.Multiply          ,    "*"         );
        public static readonly Token Divide             = TokenBuilder.ToSymbol(TokenTypes.Divide            ,    "/"         );
        public static readonly Token Percent            = TokenBuilder.ToSymbol(TokenTypes.Percent           ,    "%"         );
        public static readonly Token LessThan           = TokenBuilder.ToSymbol(TokenTypes.LessThan          ,    "<"         );
        public static readonly Token LessThanOrEqual    = TokenBuilder.ToSymbol(TokenTypes.LessThanOrEqual   ,    "<="        );
        public static readonly Token MoreThan           = TokenBuilder.ToSymbol(TokenTypes.MoreThan          ,    ">"         );
        public static readonly Token MoreThanOrEqual    = TokenBuilder.ToSymbol(TokenTypes.MoreThanOrEqual   ,    ">="        );
        public static readonly Token EqualEqual         = TokenBuilder.ToSymbol(TokenTypes.EqualEqual        ,    "=="        );
        public static readonly Token NotEqual           = TokenBuilder.ToSymbol(TokenTypes.NotEqual          ,    "!="        );
        public static readonly Token LogicalAnd         = TokenBuilder.ToSymbol(TokenTypes.LogicalAnd        ,    "&&"        );
        public static readonly Token LogicalOr          = TokenBuilder.ToSymbol(TokenTypes.LogicalOr         ,    "||"        );
        public static readonly Token LogicalNot         = TokenBuilder.ToSymbol(TokenTypes.LogicalNot        ,    "!"         );
        public static readonly Token Question           = TokenBuilder.ToSymbol(TokenTypes.Question          ,    "?"         );
        public static readonly Token Increment          = TokenBuilder.ToSymbol(TokenTypes.Increment         ,    "++"        );
        public static readonly Token Decrement          = TokenBuilder.ToSymbol(TokenTypes.Decrement         ,    "--"        );
        public static readonly Token IncrementAdd       = TokenBuilder.ToSymbol(TokenTypes.IncrementAdd      ,    "+="        );
        public static readonly Token IncrementSubtract  = TokenBuilder.ToSymbol(TokenTypes.IncrementSubtract ,    "-="        );
        public static readonly Token IncrementMultiply  = TokenBuilder.ToSymbol(TokenTypes.IncrementMultiply ,    "*="        );
        public static readonly Token IncrementDivide    = TokenBuilder.ToSymbol(TokenTypes.IncrementDivide   ,    "/="        );        
        public static readonly Token LeftBrace          = TokenBuilder.ToSymbol(TokenTypes.LeftBrace         ,    "{"         );
        public static readonly Token RightBrace         = TokenBuilder.ToSymbol(TokenTypes.RightBrace        ,    "}"         );
        public static readonly Token LeftParenthesis    = TokenBuilder.ToSymbol(TokenTypes.LeftParenthesis   ,    "("         );
        public static readonly Token RightParenthesis   = TokenBuilder.ToSymbol(TokenTypes.RightParenthesis  ,    ")"         );
        public static readonly Token LeftBracket        = TokenBuilder.ToSymbol(TokenTypes.LeftBracket       ,    "["         );
        public static readonly Token RightBracket       = TokenBuilder.ToSymbol(TokenTypes.RightBracket      ,    "]"         );
        public static readonly Token Semicolon          = TokenBuilder.ToSymbol(TokenTypes.Semicolon         ,    ";"         );
        public static readonly Token Comma              = TokenBuilder.ToSymbol(TokenTypes.Comma             ,    ","         );
        public static readonly Token Dot                = TokenBuilder.ToSymbol(TokenTypes.Dot               ,    "."         );
        public static readonly Token Colon              = TokenBuilder.ToSymbol(TokenTypes.Colon             ,    ":"         );
        public static readonly Token Assignment         = TokenBuilder.ToSymbol(TokenTypes.Assignment        ,    "="         );
        public static readonly Token Dollar             = TokenBuilder.ToSymbol(TokenTypes.Dollar            ,    "$"         );
        public static readonly Token At                 = TokenBuilder.ToSymbol(TokenTypes.At                ,    "@"         );
        public static readonly Token Pound              = TokenBuilder.ToSymbol(TokenTypes.Pound             ,    "#"         );
        public static readonly Token Pipe               = TokenBuilder.ToSymbol(TokenTypes.Pipe              ,    "|"         );
        public static readonly Token BackSlash          = TokenBuilder.ToSymbol(TokenTypes.BackSlash         ,    "\\"        ); 


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
            AddToLookup( Var                   );
            AddToLookup( If                    );
            AddToLookup( Else                  );
            AddToLookup( For                   );
            AddToLookup( While                 );
            AddToLookup( Function              );
            AddToLookup( Break                 );
            AddToLookup( Continue              );
            AddToLookup( New                   );
            AddToLookup( Return                );
            AddToLookup( Try                   );
            AddToLookup( Catch                 );
            AddToLookup( Throw                 );
            AddToLookup( In                    );
            AddToLookup( Run                   );
            AddToLookup( Then                  );
            
            AddToLookup(  Plus                 );
            AddToLookup(  Minus                );
            AddToLookup(  Multiply             );
            AddToLookup(  Divide               );
            AddToLookup(  Percent              );
            AddToLookup(  LessThan             );
            AddToLookup(  LessThanOrEqual      );
            AddToLookup(  MoreThan             );
            AddToLookup(  MoreThanOrEqual      );
            AddToLookup(  EqualEqual           );
            AddToLookup(  NotEqual             );
            AddToLookup(  LogicalAnd           );
            AddToLookup(  LogicalOr            );
            AddToLookup(  LogicalNot           );
            AddToLookup(  Question             );
            AddToLookup(  Increment            );
            AddToLookup(  Decrement            );
            AddToLookup(  IncrementAdd         );
            AddToLookup(  IncrementSubtract    );
            AddToLookup(  IncrementMultiply    );
            AddToLookup(  IncrementDivide      );
            AddToLookup(  LeftBrace            );
            AddToLookup(  RightBrace           );
            AddToLookup(  LeftParenthesis      );
            AddToLookup(  RightParenthesis     );
            AddToLookup(  LeftBracket          );
            AddToLookup(  RightBracket         );
            AddToLookup(  Semicolon            );
            AddToLookup(  Comma                );
            AddToLookup(  Dot                  );
            AddToLookup(  Colon                );
            AddToLookup(  Assignment           );
            AddToLookup(  Dollar               );
            AddToLookup(  At                   );
            AddToLookup(  Pound                );
            AddToLookup(  Pipe                 );
            AddToLookup(  BackSlash            );

            AddToLookup( True                  );
            AddToLookup( False                 );
            AddToLookup( Null                  );
            AddToLookup( WhiteSpace            );
            AddToLookup( NewLine               );
            AddToLookup( CommentSLine          );
            AddToLookup( CommentMLine          );

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