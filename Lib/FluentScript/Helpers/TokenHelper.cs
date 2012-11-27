using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using ComLib.Lang.Types;
using ComLib.Lang.Core;


namespace ComLib.Lang.Helpers
{
    /// <summary>
    /// Helper class for datatypes.
    /// </summary>
    public class TokenHelper
    {
        /// <summary>
        /// Converts from c# datatypes to fluentscript datatypes inside
        /// </summary>
        /// <param name="val"></param>
        public static LObject ConvertToLangLiteral(Token token)
        {
            if (token.Type == TokenTypes.Null) 
                return LObjects.Null;

            var type = token.Type;
            var kind = token.Kind;
            if (type == TokenTypes.LiteralNumber)       
                return new LNumber(Convert.ToDouble(token.Value));

            if (type == TokenTypes.LiteralString)
                return new LString(Convert.ToString(token.Value));

            if (type == TokenTypes.LiteralDate)
                return new LDate(Convert.ToDateTime(token.Value));

            if (type == TokenTypes.LiteralTime)
                return new LTime((TimeSpan)token.Value);

            if (type == TokenTypes.LiteralDay)
                return new LDayOfWeek((DayOfWeek)token.Value);

            if (kind == TokenKind.LiteralBool)
                return new LBool(Convert.ToBoolean(token.Value));

            return new LClass(token.Value);
        }
    }
}
