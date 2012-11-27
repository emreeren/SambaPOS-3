using System;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.Helpers;
using ComLib.Lang.Types;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class BinaryExpr : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="left">Left hand expression</param>
        /// <param name="op">Operator</param>
        /// <param name="right">Right expression</param>
        public BinaryExpr(Expr left, Operator op, Expr right)
        {
            this.Nodetype = NodeTypes.SysBinary;
            this.Left = left;
            this.Right = right;
            this.AddChild(left);
            this.AddChild(right);
            this.Op = op;
        }


        /// <summary>
        /// Left hand expression
        /// </summary>
        public Expr Left;


        /// <summary>
        /// Operator * - / + % 
        /// </summary>
        public Operator Op;


        /// <summary>
        /// Right hand expression
        /// </summary>
        public Expr Right;


        /// <summary>
        /// Evaluate * / + - % 
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            // Validate
            object result = 0;
            var left =  (LObject)Left.Evaluate();
            var right = (LObject)Right.Evaluate();
            
            // Case 1: Both numbers
            if (this.IsTypeMatch(LTypes.Number, left, right))
            {
                result = EvalHelper.CalcNumbers(this, (LNumber)left, (LNumber)right, Op);
            }
            // Case 2: Both times
            else if (this.IsTypeMatch(LTypes.Time, left, right))
            {
                result = EvalHelper.CalcTimes(this, (LTime)left, (LTime)right, Op);
            }
            // Case 3: Both dates
            else if (this.IsTypeMatch(LTypes.Date, left, right))
            {
                result = EvalHelper.CalcDates(this, (LDate)left, (LDate)right, Op);
            }
            // Case 4: Both strings.
            else if (this.IsTypeMatch(LTypes.String, left, right))
            {
                var strleft =  ((LString) left).Value;
                var strright = ((LString) right).Value;

                // Check string limit.
                Ctx.Limits.CheckStringLength(this, strleft, strright);
                result = new LString(strleft + strright);
            }
                
            // MIXED TYPES
            // TODO: Needs to be improved with new code for types.
            // Case 5 : Double and Bool
            else if (left.Type == LTypes.Number && right.Type == LTypes.Bool)
            {
                var r = ((LBool) right).Value;
                var rval = r ? 1 : 0;
                result = EvalHelper.CalcNumbers(this, (LNumber)left, new LNumber(rval), Op);
            }
            // Bool Double
            else if (left.Type == LTypes.Bool && right.Type == LTypes.Number)
            {
                var l = ((LBool) left).Value;
                var lval = l ? 1 : 0;
                result = EvalHelper.CalcNumbers(this, new LNumber(lval), (LNumber)right, Op);
            }
            // Append as strings.
            else if (left.Type == LTypes.String && right.Type == LTypes.Bool)
            {
                var st1 = ((LString)left).Value + ((LBool)right).Value.ToString().ToLower();
                result = new LString(st1);
            }
            // Append as strings.
            else if (left.Type == LTypes.Bool && right.Type == LTypes.String)
            {
                var st2 = ((LBool) left).Value.ToString().ToLower() + ((LString) right).Value;
                result = new LString(st2);
            }
            // TODO: Need to handle LUnit and LVersion better
            //else if (left.Type == LTypes.Unit && right.Type == LTypes.Unit)
            else if (left.Type.Name == "LUnit" && right.Type.Name == "LUnit")
            {
                result = EvalHelper.CalcUnits(this, (LUnit)((LClass)left).Value, (LUnit)((LClass)right).Value, Op, Ctx.Units);
            }
            else
            {
                var st3 = left.GetValue().ToString() + right.GetValue().ToString();
                result = new LString(st3);
            }
            return result;
        }


        /// <summary>
        /// Is match with the type supplied and the 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        private bool IsTypeMatch(LType type, LObject obj1, LObject obj2)
        {
            if (obj1.Type == type && obj2.Type == type)
                return true;
            return false;
        }
    }    
}
