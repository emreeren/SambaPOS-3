using System;
using System.Collections.Generic;

// <lang:using>
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.AST
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
            this.Nodetype = NodeTypes.SysNew;
            this.InitBoundary(true, ")");
            this.ParamList = new List<object>();
            this.ParamListExpressions = new List<Expr>();
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
                ParamHelper.ResolveNonNamedParameters(ParamListExpressions, ParamList);
                constructorArgs = ParamList.ToArray();
            }

            // CASE 1: Built in basic system types ( string, date, time, etc )
            if(LTypesLookup.IsBasicTypeShortName(this.TypeName))
            {
                // TODO: Move this check to Semacts later
                var langType = LTypesLookup.GetLType(this.TypeName);
                var methods = this.Ctx.Methods.Get(langType);
                var canCreate = methods.CanCreateFromArgs(constructorArgs);
                if (!canCreate)
                    throw BuildRunTimeException("Can not create " + this.TypeName + " from parameters");

                // Allow built in type methods to create it.
                var result = methods.CreateFromArgs(constructorArgs);
                return result;
            }
            // CASE 2: Custom types e.g. custom classes.
            var hostLangArgs = LangTypeHelper.ConvertToArrayOfHostLangValues(constructorArgs);
            var instance = Ctx.Types.Create(this.TypeName, hostLangArgs);
            return new LClass(instance);
        }
    }
}
