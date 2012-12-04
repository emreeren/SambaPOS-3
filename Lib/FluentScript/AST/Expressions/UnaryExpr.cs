using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.Types;
using ComLib.Lang.Parsing;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class UnaryExpr : VariableExpr
    {
        /// <summary>
        /// The increment value.
        /// </summary>
        public double Increment;


        /// <summary>
        /// The operator.
        /// </summary>
        public Operator Op;


        /// <summary>
        /// The expression to apply a unary symbol on. e.g. !
        /// </summary>
        public Expr Expression;


        /// <summary>
        /// Initialize
        /// </summary>
        public UnaryExpr()
        {
            this.Nodetype = NodeTypes.SysUnary;
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

            var valobj = (LObject)this.Ctx.Memory.Get<object>(this.Name);

            // Double ? 
            if (valobj.Type == LTypes.Number ) 
                return IncrementNumber((LNumber)valobj);

            // String ?
            if (valobj.Type == LTypes.String) 
                return IncrementString((LString)valobj);

            throw new LangException("Syntax Error", "Unexpected operation", Ref.ScriptName, Ref.Line, Ref.CharPos);
        }


        private LString IncrementString(LString sourceVal)
        {
            // Check 1: Can only do += on strings.
            if (Op != Operator.PlusEqual)
                throw new LangException("Syntax Error", "string operation with " + Op.ToString() + " not supported", Ref.ScriptName, Ref.Line, Ref.CharPos);

            this.DataType = typeof(string);
            var val = this.Expression.Evaluate() as LObject;

            // Check 2: Check for null
            if (val == LObjects.Null)
                return sourceVal;

            // Check 3: Limit size if string
            Ctx.Limits.CheckStringLength(this, sourceVal.Value, val.GetValue().ToString());

            // Finally do the appending.
            string appended = sourceVal.Value + val.GetValue().ToString();
            sourceVal.Value = appended;
            this.Value = appended;
            this.Ctx.Memory.SetValue(this.Name, sourceVal);
            return sourceVal;
        }


        private LNumber IncrementNumber(LNumber val)
        {
            this.DataType = typeof(double);
            var inc = this.Increment == 0 ? 1 : this.Increment;
            if (this.Expression != null)
            {
                var incval = this.Expression.Evaluate();
                // TODO: Check if null and throw langexception?
                inc = ((LNumber)incval).Value;
            }

            // 1. Calculate the unary value
            val = EvalHelper.CalcUnary(val, Op, inc);

            // 2. Set the value back into scope
            this.Value = val;
            this.Ctx.Memory.SetValue(this.Name, val);
            return val;
        }


        private object HandleLogicalNot()
        {
            var result = this.Expression.Evaluate() as LObject;
            
            // Check 1:  This is actually an assert and should not happen.
            if (result == null)
                throw this.BuildRunTimeException("Null value encountered");

            var retVal = false;
            
            // Only handle bool for logical not !true !false
            if (result.Type == LTypes.Bool)
                retVal = !((LBool)result).Value;
            else if (result == LObjects.Null)
                retVal = true;

            return new LBool(retVal);
        }
    }
}
