using System;
using System.Collections.Generic;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.AST
{        
    /// <summary>
    /// Function call expression data.
    /// </summary>
    public class ParameterExpr : Expr, IParameterExpression
    {
        /// <summary>
        /// Metadata about the parameters.
        /// </summary>
        protected FunctionMetaData _fmeta;


        /// <summary>
        /// Function call expression
        /// </summary>
        public ParameterExpr()
        {
            this.Init(null);
        }


        /// <summary>
        /// Initailizes with function metadata.
        /// </summary>
        /// <param name="meta"></param>
        public void Init(FunctionMetaData meta)        
        {
            _fmeta = meta;
            ParamList = new List<object>();
            ParamListExpressions = new List<Expr>();
        }


        /// <summary>
        /// List of expressions.
        /// </summary>
        public List<Expr> ParamListExpressions { get; set; }


        /// <summary>
        /// List of arguments.
        /// </summary>
        public List<object> ParamList { get; set; }


        /// <summary>
        /// Resolves the parameters.
        /// </summary>
        protected void ResolveParams()
        {
            ParamHelper.ResolveParametersForScriptFunction(_fmeta, this.ParamListExpressions, this.ParamList);
        }


        /// <summary>
        /// Gets a parameter value if available or the default value otherwise. Index out of bounds will throw an error.
        /// </summary>
        /// <param name="index">index position of the parameter</param>
        /// <param name="allowDefaultValue">Whether or not to enable using the default value.</param>
        /// <param name="defaultValue">A default value to use if the parameter is not available.</param>
        /// <returns></returns>
        protected object GetParamValue(int index, bool allowDefaultValue, object defaultValue)
        {
            bool hasParams = this.ParamList != null && this.ParamList.Count > 0;
            if (!hasParams && !allowDefaultValue)
                throw BuildRunTimeException("No parameters available for custom function plugin : " + this._fmeta.Name);

            if (hasParams && this.ParamList.Count <= index)
                throw BuildRunTimeException("Unexpected parameter retrieval attempted in custom function plugin : " + this._fmeta.Name);
            
            if (!hasParams && allowDefaultValue) 
                return defaultValue;

            return this.ParamList[index];
        }


        /// <summary>
        /// Gets a parameter value if available or the default value otherwise. Index out of bounds will throw an error.
        /// </summary>
        /// <param name="index">index position of the parameter</param>
        /// <param name="allowDefaultValue">Whether or not to enable using the default value.</param>
        /// <param name="defaultValue">A default value to use if the parameter is not available.</param>
        /// <returns></returns>
        private object GetParamValueRaw(int index, bool allowDefaultValue, object defaultValue)
        {
            bool hasParams = this.ParamList != null && this.ParamList.Count > 0;
            if (!hasParams && !allowDefaultValue)
                throw BuildRunTimeException("No parameters available for custom function plugin : " + this._fmeta.Name);

            if (hasParams && this.ParamList.Count <= index)
                throw BuildRunTimeException("Unexpected parameter retrieval attempted in custom function plugin : " + this._fmeta.Name);

            if (!hasParams && allowDefaultValue)
                return defaultValue;

            var result = (LObject)this.ParamList[index];
            return result.GetValue();
        }


        /// <summary>
        /// Gets the parameter value as a datetime.
        /// </summary>
        /// <param name="index">The index position of the parameter</param>
        /// <param name="allowDefaultValue">Whether or not to allow using the default value supplied if the parameter does not exist</param>
        /// <param name="defaultValue">A default value to use if the parameter does not exist ( index out of range )</param>
        /// <returns></returns>
        protected string GetParamValueString(int index, bool allowDefaultValue, string defaultValue)
        {
            var val = (string)this.GetParamValueRaw(index, allowDefaultValue, defaultValue);
            return val;
        }


        /// <summary>
        /// Gets the parameter value as a datetime.
        /// </summary>
        /// <param name="index">The index position of the parameter</param>
        /// <param name="allowDefaultValue">Whether or not to allow using the default value supplied if the parameter does not exist</param>
        /// <param name="defaultValue">A default value to use if the parameter does not exist ( index out of range )</param>
        /// <returns></returns>
        protected double GetParamValueNumber(int index, bool allowDefaultValue, double defaultValue)
        {
            var val = (double)this.GetParamValueRaw(index, allowDefaultValue, defaultValue);
            return val;
        }


        /// <summary>
        /// Gets the parameter value as a datetime.
        /// </summary>
        /// <param name="index">The index position of the parameter</param>
        /// <param name="allowDefaultValue">Whether or not to allow using the default value supplied if the parameter does not exist</param>
        /// <param name="defaultValue">A default value to use if the parameter does not exist ( index out of range )</param>
        /// <returns></returns>
        protected bool GetParamValueBool(int index, bool allowDefaultValue, bool defaultValue)
        {
            var val = (bool)this.GetParamValueRaw(index, allowDefaultValue, defaultValue);
            return val;
        }


        /// <summary>
        /// Gets the parameter value as a datetime.
        /// </summary>
        /// <param name="index">The index position of the parameter</param>
        /// <param name="allowDefaultValue">Whether or not to allow using the default value supplied if the parameter does not exist</param>
        /// <param name="defaultValue">A default value to use if the parameter does not exist ( index out of range )</param>
        /// <returns></returns>
        protected DateTime GetParamValueDate(int index, bool allowDefaultValue, DateTime defaultValue)
        {
            var val = (DateTime)this.GetParamValueRaw(index, allowDefaultValue, defaultValue);
            return val;
        }
    }
}
