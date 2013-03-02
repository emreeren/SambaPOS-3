#pragma warning disable 1591
using System;


namespace ComLib.Lang.AST
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

}