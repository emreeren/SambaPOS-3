using System.Collections.Generic;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser
{
    /// <summary>
    /// Terminators used for parsing expressions/statements.
    /// </summary>
    public class Terminators
    {
        /// <summary>
        /// Used for end of expression;
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpStatementEnd = new Dictionary<Token, bool>()
        {
            { Tokens.NewLine, true },
            { Tokens.Semicolon, true },
        };


        /// <summary>
        /// Used for end of expression;
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpStatementEnd2 = new Dictionary<Token, bool>()
        {
            { Tokens.NewLine, true },
            { Tokens.Semicolon, true },
            { Tokens.EndToken, true }
        };


        /// <summary>
        /// Used for end of expression;
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpSemicolonEnd = new Dictionary<Token, bool>()
        {
            { Tokens.Semicolon, true },
        };


        /// <summary>
        /// Used for end of expression;
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpVarDeclarationEnd = new Dictionary<Token, bool>()
        {
            { Tokens.Comma, true },
            { Tokens.NewLine, true },
            { Tokens.Semicolon, true }
        };


        /// <summary>
        /// Used for end of expression;
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpSingle = new Dictionary<Token, bool>()
        {
            { Tokens.Comma, true }
        };


        /// <summary>
        /// Used for end of ( ) in if
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpParenthesisEnd = new Dictionary<Token, bool>()
        {
            { Tokens.RightParenthesis, true }
        };


        /// <summary>
        /// Used for end of ( ) in if
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpParenthesisNewLineEnd = new Dictionary<Token, bool>()
        {
            { Tokens.RightParenthesis, true },
            { Tokens.NewLine, true },
        };


        /// <summary>
        /// Used for end of condition in if
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpThenEnd = new Dictionary<Token, bool>()
        {
            { Tokens.Then, true },
            { Tokens.LeftBrace, true },
            { Tokens.NewLine, true },
        };


        /// <summary>
        /// Used for end of array index ]
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpBracketEnd = new Dictionary<Token, bool>()
        {
            { Tokens.RightBracket, true }
        };


        /// <summary>
        /// Used for end of parsing expressions in array
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpArrayDeclareEnd = new Dictionary<Token, bool>()
        {
            { Tokens.RightBracket, true },
            { Tokens.Comma, true }
        };


        /// <summary>
        /// Used for end of parsing expressions in array
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpMapDeclareEnd = new Dictionary<Token, bool>()
        {
            { Tokens.RightBrace, true },
            { Tokens.Comma, true }
        };


        /// <summary>
        /// Used to terminate parsing for function call.
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpFuncExpEnd = new Dictionary<Token, bool>()
        {
            { Tokens.Comma, true },
            { Tokens.RightParenthesis, true }
        };


        /// <summary>
        /// Used to terminate a fluent expression
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpFluentFuncExpParenEnd = new Dictionary<Token, bool>()
        {
            { Tokens.Comma, true },
            { Tokens.Semicolon, true },
            { Tokens.RightParenthesis, true },
            { Tokens.NewLine, true },
        };


        /// <summary>
        /// Used to terminate a fluent expression
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpFlexibleEnd = new Dictionary<Token, bool>()
        {
            { Tokens.Comma, true },
            { Tokens.Semicolon, true },
            { Tokens.RightParenthesis, true },
            { Tokens.NewLine, true },
            { Tokens.RightBrace, true },
            { Tokens.RightBracket, true }
        };


        /// <summary>
        /// Used to terminate a fluent expression
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpFluentFuncExpNoParenEnd = new Dictionary<Token, bool>()
        {
            { Tokens.Comma, true },
            { Tokens.Semicolon, true },
            { Tokens.NewLine, true },
        };


        /// <summary>
        /// Use in separating expressions using and/or.
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpPartEnd = new Dictionary<Token, bool>()
        {
            { Tokens.Semicolon, true },
            { Tokens.Then, true },
        };


        /// <summary>
        /// Used for Math precedence shunting yard algorithm.
        /// </summary>
        public static readonly IDictionary<Token, bool> ExpMathShuntingYard = new Dictionary<Token, bool>()
        {
            { Tokens.Plus, true },
            { Tokens.Minus, true },
            { Tokens.Multiply, true },
            { Tokens.Divide, true },
            { Tokens.Percent, true },
            { Tokens.LeftParenthesis, true},
            { Tokens.RightParenthesis, true},
            { Tokens.LessThan, true},
            { Tokens.LessThanOrEqual, true},
            { Tokens.MoreThan, true},
            { Tokens.MoreThanOrEqual, true},
            { Tokens.EqualEqual, true},
            { Tokens.NotEqual, true},
            { Tokens.LogicalOr, true},
            { Tokens.LogicalAnd, true},
        };
    }
}
