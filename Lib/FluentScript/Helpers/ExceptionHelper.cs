using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper classs for throwing language exceptions
    /// </summary>
    public class ExceptionHelper
    {
        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <returns></returns>
        public static LangException BuildRunTimeException(AstNode node, string message)
        {
            return new LangException("Runtime Error", message, node.Ref.ScriptName, node.Ref.Line, node.Ref.CharPos);
        }


        /// <summary>
        /// Checks for null object and throws a lang exception with the message supplied.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        public static void NotNull(AstNode node, object obj, string message)
        {
            if (obj == null || obj == LObjects.Null)
            {
                throw new LangException("Runtime Error", "Can not perform " + message + " on null object",
                    node.Ref.ScriptName, node.Ref.Line, node.Ref.CharPos);
            }
        }


        /// <summary>
        /// Checks for null object and throws a lang exception with the message supplied.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        public static void NotNullWithMessage(AstNode node, object obj, string message)
        {
            if (obj == null || obj == LObjects.Null)
            {
                throw new LangException("Runtime Error", message,
                    node.Ref.ScriptName, node.Ref.Line, node.Ref.CharPos);
            }
        }

        
        /// <summary>
        /// Expects the objects type to be of the supplied type.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="result"></param>
        /// <param name="lType"></param>
        public static void ExpectType(AstNode node, object result, LType lType)
        {
            if(!(result is LObject) || ((LObject)result).Type != lType)
            {
                throw new LangException("Runtime Error", "Expected type " + lType.Name,
                   node.Ref.ScriptName, node.Ref.Line, node.Ref.CharPos);
            }
        }


        /// <summary>
        /// Expects the objects type to be of the supplied type.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="result"></param>
        /// <param name="lType"></param>
        public static void NotNullType(AstNode node, object obj, string nullMessage, LType lType)
        {
            if (obj == null || obj == LObjects.Null)
            {
                throw new LangException("Runtime Error", nullMessage,
                    node.Ref.ScriptName, node.Ref.Line, node.Ref.CharPos);
            }
            if (!(obj is LObject) || ((LObject)obj).Type != lType)
            {
                throw new LangException("Runtime Error", "Expected type " + lType.Name,
                   node.Ref.ScriptName, node.Ref.Line, node.Ref.CharPos);
            }
        }
    }
}
