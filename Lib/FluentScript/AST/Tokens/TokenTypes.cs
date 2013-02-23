#pragma warning disable 1591
using System;


namespace ComLib.Lang.AST
{
    /// <summary>
    /// Represents ids for tokens
    /// </summary>
    public class TokenTypes
    {
        // Keywords
        public const int Var                = 100;
        public const int If                 = 101;
        public const int Else               = 102;
        public const int Break              = 103;
        public const int Continue           = 104;
        public const int For                = 105;
        public const int While              = 106;
        public const int Function           = 107;
        public const int Return             = 108;
        public const int New                = 109;
        public const int Try                = 110;
        public const int Catch              = 111;
        public const int Throw              = 112;
        public const int In                 = 113;
        public const int Run                = 114;
        public const int Then               = 115;

        public const int True               = 200;
        public const int False              = 201;
        public const int Null               = 202;
        public const int WhiteSpace         = 203;
        public const int NewLine            = 204;
        public const int CommentSLine       = 205;
        public const int CommentMLine       = 206;
        public const int Ident              = 207;
        public const int LiteralString      = 208;
        public const int LiteralNumber      = 209;
        public const int LiteralDate        = 210;
        public const int LiteralOther       = 211;
        public const int LiteralBool        = 212;
        public const int LiteralVersion     = 213;
        public const int LiteralTime        = 214;
        public const int LiteralDay         = 215;

        // Comparision operators ( < <= > >= == != )
        public const int Plus               = 300;
        public const int Minus              = 301;
        public const int Multiply           = 302;
        public const int Divide             = 303;
        public const int Percent            = 304;
		public const int LessThan 			= 305;
        public const int LessThanOrEqual 	= 306;
        public const int MoreThan 			= 307;
        public const int MoreThanOrEqual 	= 308;
        public const int EqualEqual 		= 309;
        public const int NotEqual 			= 310;
        public const int LogicalAnd 		= 311;
        public const int LogicalOr 			= 312;
        public const int LogicalNot 		= 313;
        public const int Question    		= 314;
        public const int Increment 			= 315;
        public const int Decrement 			= 316;
        public const int IncrementAdd 		= 317;
        public const int IncrementSubtract 	= 318;
        public const int IncrementMultiply 	= 319;
        public const int IncrementDivide 	= 320;
        public const int LeftBrace 			= 321;
        public const int RightBrace 		= 322;
        public const int LeftParenthesis 	= 323;
        public const int RightParenthesis 	= 324;
        public const int LeftBracket 		= 325;
        public const int RightBracket 		= 326;
        public const int Semicolon 			= 327;
        public const int Comma 				= 328;
        public const int Dot 				= 329;
        public const int Colon 				= 330;
        public const int Assignment 		= 331;
        public const int Dollar 			= 332;
        public const int At 				= 333;
        public const int Pound 				= 334;
        public const int Pipe 				= 335;
        public const int BackSlash 			= 336;

        public const int EOF                = 400;
        public const int Multi              = 402;
        public const int Unknown            = 403;
    }


}