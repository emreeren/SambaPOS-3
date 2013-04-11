using System.Collections.Generic;

namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// Token for the language.
    /// </summary>
    public enum Operator
    {
        /* BINARY + - * / %  */
        /// <summary>
        /// +
        /// </summary>
        Add,


        /// <summary>
        /// -
        /// </summary>
        Subtract,
        
        
        /// <summary>
        /// * 
        /// </summary>
        Multiply,


        /// <summary>
        /// /
        /// </summary>
        Divide,


        /// <summary>
        /// %
        /// </summary>
        Modulus,


        /* COMPARE < <= > >= != ==  */
        /// <summary>
        /// &lt;
        /// </summary>
        LessThan,


        /// <summary>
        /// &lt;=
        /// </summary>
        LessThanEqual,


        /// <summary>
        /// >
        /// </summary>
        MoreThan,


        /// <summary>
        /// >=
        /// </summary>
        MoreThanEqual,

                
        /// <summary>
        /// =
        /// </summary>
        Equal,


        /// <summary>
        /// ==
        /// </summary>
        EqualEqual,


        /// <summary>
        /// !=
        /// </summary>
        NotEqual,


        /* CONDITION && ||  */
        /// <summary>
        /// and
        /// </summary>
        And,

        /// <summary>
        /// ||
        /// </summary>
        Or,


        /* UNARY ++ -- += -= *= /= */
        /// <summary>
        /// ++
        /// </summary>
        PlusPlus,


        /// <summary>
        /// --
        /// </summary>
        MinusMinus,


        /// <summary>
        /// += 
        /// </summary>
        PlusEqual,


        /// <summary>
        /// -=
        /// </summary>
        MinusEqual,


        /// <summary>
        /// *=
        /// </summary>
        MultEqual,


        /// <summary>
        /// /=
        /// </summary>
        DivEqual,


        /// <summary>
        /// (
        /// </summary>
        LeftParenthesis,

                
        /// <summary>
        /// {
        /// </summary>
        LeftBrace,


        /// <summary>
        /// [
        /// </summary>
        LeftBracket,


        /// <summary>
        /// )
        /// </summary>
        RightParenthesis,

        
        /// <summary>
        /// ]
        /// </summary>
        RightBracket,


        /// <summary>
        /// }
        /// </summary>
        RightBrace,

        
        /// <summary>
        /// ,
        /// </summary>
        Comma,


        /// <summary>
        /// !
        /// </summary>
        LogicalNot,


        /// <summary>
        /// .
        /// </summary>
        Dot
    }



    /// <summary>
    /// Operator lookup class
    /// </summary>
    public class Operators
    {
        internal static IDictionary<string, Operator> AllOps = new Dictionary<string, Operator>()
        {
            { "*", Operator.Multiply },
            { "/", Operator.Divide }, 
            { "+", Operator.Add },
            { "-", Operator.Subtract },
            { "%", Operator.Modulus },
            { "<", Operator.LessThan },
            { "<=", Operator.LessThanEqual },
            { ">", Operator.MoreThan },
            { ">=", Operator.MoreThanEqual },
            { "==", Operator.EqualEqual },
            { "!=", Operator.NotEqual },
            { "=", Operator.Equal },
            { "&&", Operator.And },
            { "||", Operator.Or },
            { "(", Operator.LeftParenthesis },
            { ")", Operator.RightParenthesis },
            { "{", Operator.LeftBrace },
            { "}", Operator.RightBrace },
            { "[", Operator.LeftBracket },
            { "]", Operator.RightBracket },
            { "++", Operator.PlusPlus },
            { "--", Operator.MinusMinus },
            { "+=", Operator.PlusEqual },
            { "-=", Operator.MinusEqual },
            { "*=", Operator.MultEqual },
            { "/=", Operator.DivEqual },
            { ",",  Operator.Comma },
            { "!", Operator.LogicalNot },
            { ".", Operator.Dot }
        };


        internal static IDictionary<Operator, bool> MathOps = new Dictionary<Operator, bool>()
        {
            { Operator.Multiply, true },
            { Operator.Divide, true },
            { Operator.Add, true },
            { Operator.Subtract, true },
            { Operator.Modulus, true },
        };


        internal static IDictionary<Operator, bool> CompareOps = new Dictionary<Operator, bool>()
        {
            { Operator.LessThan, true },
            { Operator.LessThanEqual, true },
            { Operator.MoreThan, true },
            { Operator.MoreThanEqual, true },
            { Operator.EqualEqual, true },
            { Operator.NotEqual, true },
        };


        internal static IDictionary<Operator, bool> IncrementOps = new Dictionary<Operator, bool>()
        {
            { Operator.PlusPlus, true },
            { Operator.PlusEqual, true },
            { Operator.MinusMinus, true },
            { Operator.MinusEqual, true },
            { Operator.MultEqual, true },
            { Operator.DivEqual, true },
        };


        internal static IDictionary<string, int> OpsPrecedence = new Dictionary<string, int>()
        {
            { "(",       9 },
            { ")",       9 },
            { "*",       7 },
            { "/",       7 },
            { "%",       7 },
            { "+",       6 },
            { "-",       6 },
            { "<",       4 },
            { "<=",      4 },
            { ">",       4 },
            { ">=",      4 },
            { "!=",      1 },
            { "==",      1 },
            { "&&",      0 },
            { "||",      0 }
        };


        /// <summary>
        /// Whether or not this following text is an operator that has precedence value.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static bool IsOp(string op)
        {
            return OpsPrecedence.ContainsKey(op);
        }


        /// <summary>
        /// Get the operator as an enum
        /// </summary>
        /// <returns></returns>
        public static Operator ToOp(string op)
        {
            return AllOps[op];
        }


        /// <summary>
        /// Checks if the operator supplied is a binary op ( * / + - % )
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static bool IsMath(Operator op)
        {
            return MathOps.ContainsKey(op);
        }


        /// <summary>
        /// Checks if the operator is a conditional 
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static bool IsConditional(Operator op)
        {
            return op == Operator.And || op == Operator.Or;
        }


        /// <summary>
        /// Checks if the operator is a comparison operator ( less lessthan more morethan equal not equal ).
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static bool IsCompare(Operator op)
        {
            return CompareOps.ContainsKey(op);
        }


        /// <summary>
        /// Checks if the operator supplied is a binary op ( * / + - % )
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static bool IsIncrement(Operator op)
        {
            return IncrementOps.ContainsKey(op);
        }


        /// <summary>
        /// Checks if the operator supplied is a binary op ( * / + - % )
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static bool IsLogical(Operator op)
        {
            return op == Operator.And || op == Operator.Or;
        }


        /// <summary>
        /// Gets the operator precendence.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static int Precedence(string token)
        {
            int precedence = OpsPrecedence[token];
            return precedence;
        }
    }

}
