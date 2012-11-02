using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang
{
    /// <summary>
    /// Condition expression less, less than equal, more, more than equal etc.
    /// </summary>
    public class ConditionExpr : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="left">Left hand expression</param>
        /// <param name="op">Operator</param>
        /// <param name="right">Right expression</param>
        public ConditionExpr(Expr left, Operator op, Expr right)
        {
            Left = left;
            Right = right;
            Op = op;
        }


        /// <summary>
        /// Left hand expression
        /// </summary>
        public Expr Left;


        /// <summary>
        /// Operator > >= == != less less than
        /// </summary>
        public Operator Op;


        /// <summary>
        /// Right hand expression
        /// </summary>
        public Expr Right;


        /// <summary>
        /// Evaluate > >= != == less less than
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            // Validate
            if (Op != Operator.And && Op != Operator.Or)
                throw new ArgumentException("Only && || supported");

            bool result = false;
            bool left = Left.EvaluateAs<bool>();
            bool right = Right.EvaluateAs<bool>();

            if (Op == Operator.Or)
            {
                result = left || right;
            }
            else if (Op == Operator.And)
            {
                result = left && right;
            }
            return result;
        }
    }    
}
