using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class UnaryExpr : VariableExpr
    {
        private double Increment;
        private Operator Op;

        /// <summary>
        /// The expression to apply a unary symbol on. e.g. !
        /// </summary>
        public Expr Expression;


        /// <summary>
        /// Initialize
        /// </summary>
        public UnaryExpr()
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="incValue">Value to increment</param>
        /// <param name="op">The unary operator</param>
        /// <param name="name">Variable name</param>
        /// <param name="ctx">Context of the script</param>
        public UnaryExpr(string name, double incValue, Operator op, Context ctx)
        {
            this.Name = name;
            this.Op = op;
            this.Increment = incValue;
            this.Ctx = ctx;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="exp">Expression representing value to increment by</param>
        /// <param name="op">The unary operator</param>
        /// <param name="name">Variable name</param>
        /// <param name="ctx">Context of the script</param>
        public UnaryExpr(string name, Expr exp, Operator op, Context ctx)
        {
            this.Name = name;
            this.Op = op;
            this.Expression = exp;
            this.Ctx = ctx;
        }
        

        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            // Logical not?
            if (Op == Operator.LogicalNot)
                return HandleLogicalNot();

            object valobj = this.Ctx.Memory.Get<object>(this.Name);

            // Double ? 
            if (valobj is double || valobj is int) return IncrementNumber(Convert.ToDouble(valobj));

            // String ?
            if (valobj is string) return IncrementString((string)valobj);

            throw new LangException("Syntax Error", "Unexpected operation", Ref.ScriptName, Ref.Line, Ref.CharPos);
        }


        private string IncrementString(string sourceVal)
        {
            if (Op != Operator.PlusEqual)
                throw new LangException("Syntax Error", "string operation with " + Op.ToString() + " not supported", Ref.ScriptName, Ref.Line, Ref.CharPos);

            this.DataType = typeof(string);
            string val = this.Expression.EvaluateAs<string>();

            // Check limit
            Ctx.Limits.CheckStringLength(this, sourceVal, val);

            string appended = sourceVal + val;
            this.Value = appended;
            this.Ctx.Memory.SetValue(this.Name, appended);
            return appended;
        }


        private double IncrementNumber(double val)
        {
            this.DataType = typeof(double);
            if (this.Expression != null)
                Increment = this.Expression.EvaluateAs<double>();
            else if (Increment == 0)
                Increment = 1;

            if (Op == Operator.PlusPlus)
            {
                val++;
            }
            else if (Op == Operator.MinusMinus)
            {
                val--;
            }
            else if (Op == Operator.PlusEqual)
            {
                val = val + Increment;
            }
            else if (Op == Operator.MinusEqual)
            {
                val = val - Increment;
            }
            else if (Op == Operator.MultEqual)
            {
                val = val * Increment;
            }
            else if (Op == Operator.DivEqual)
            {
                val = val / Increment;
            }            
            
            // Set the value back into scope
            this.Value = val;
            this.Ctx.Memory.SetValue(this.Name, val);

            return val;
        }


        private object HandleLogicalNot()
        {
            object result = this.Expression.Evaluate();
            if (result == null)
                return false;
            if (result.GetType() == typeof(double))
                return false;
            if (result.GetType() == typeof(string))
                return false;
            if (result.GetType() == typeof(DateTime))
                return false;
            if (result.GetType() == typeof(bool))
                return !((bool)result);
            if (result == LNull.Instance)
                return true;

            return false;
        }
    }
}
