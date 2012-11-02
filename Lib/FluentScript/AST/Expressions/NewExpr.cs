using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang.Helpers;

namespace ComLib.Lang
{
    /// <summary>
    /// New instance creation.
    /// </summary>
    public class NewExpr : Expr, IParameterExpression
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public NewExpr()
        {
            InitBoundary(true, ")");
            ParamList = new List<object>();
            ParamListExpressions = new List<Expr>();
        }


        /// <summary>
        /// Name of 
        /// </summary>
        public string TypeName { get; set; }



        /// <summary>
        /// List of expressions.
        /// </summary>
        public List<Expr> ParamListExpressions { get; set; }


        /// <summary>
        /// List of arguments.
        /// </summary>
        public List<object> ParamList { get; set; }


        /// <summary>
        /// Creates new instance of the type.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            object[] constructorArgs = null;
            if (ParamListExpressions != null && ParamListExpressions.Count > 0)
            {
                ParamList = new List<object>();
                FunctionHelper.ResolveParameters(ParamListExpressions, ParamList);
                constructorArgs = ParamList.ToArray();
            }
            if (string.Compare(TypeName, "Date", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                DateTime result = LDate.CreateFrom(constructorArgs);
                return result;
            }
            else if (string.Compare(TypeName, "Time", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                TimeSpan result = TimeTypeHelper.CreateTimeFrom(constructorArgs);
                return result;
            }
            return Ctx.Types.Create(TypeName, constructorArgs);
        }
    }
}
