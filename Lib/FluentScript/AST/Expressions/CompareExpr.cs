

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.Helpers;
using ComLib.Lang.Types;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Condition expression less, less than equal, more, more than equal etc.
    /// </summary>
    public class CompareExpr : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="left">Left hand expression</param>
        /// <param name="op">Operator</param>
        /// <param name="right">Right expression</param>
        public CompareExpr(Expr left, Operator op, Expr right)
        {
            this.Nodetype = NodeTypes.SysCompare;
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
            object result = null;
            var left =  (LObject)Left.Evaluate();
            var right = (LObject)Right.Evaluate();

            
            // Both double
            if (left.Type == LTypes.Number && right.Type == LTypes.Number)
                result = EvalHelper.CompareNumbers(this, (LNumber)left, (LNumber)right, Op);

            // Both strings
            else if (left.Type == LTypes.String && right.Type == LTypes.String)
                result = EvalHelper.CompareStrings(this, (LString)left, (LString)right, Op);

            // Both bools
            else if (left.Type == LTypes.Bool && right.Type == LTypes.Bool)
                result = EvalHelper.CompareBools(this, (LBool)left, (LBool)right, Op);

            // Both dates
            else if (left.Type == LTypes.Date && right.Type == LTypes.Date)
                result = EvalHelper.CompareDates(this, (LDate)left, (LDate)right, Op);

            // Both Timespans
            else if (left.Type == LTypes.Time && right.Type == LTypes.Time)
                result = EvalHelper.CompareTimes(this, (LTime)left, (LTime)right, Op);

            // 1 or both null
            else if (left == LObjects.Null || right == LObjects.Null)
                result = EvalHelper.CompareNull(left, right, Op);
            
            // Day of week ?
            else if (left.Type == LTypes.DayOfWeek || right.Type == LTypes.DayOfWeek)
                result = EvalHelper.CompareDays(this, left, right, Op);

            // Units
            //else if (left.Type == LTypes.Unit || right.Type == LTypes.Unit)
            else if (left.Type.Name == "LUnit" || right.Type.Name == "LUnit")
                result = EvalHelper.CompareUnits(this, (LUnit)((LClass)left).Value, (LUnit)((LClass)right).Value, Op);

            return result;
        }
    }    
}
