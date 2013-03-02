using System;
using System.Collections.Generic;

using ComLib.Lang.Core;

namespace ComLib.Lang.AST
{
    public class TokenBuilder
    {
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
            var lv = new ComLib.Lang.Types.LVersion(val);
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
        /// <param name="tokens">The tokens making up the interpolated token.</param>
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
    }
}
