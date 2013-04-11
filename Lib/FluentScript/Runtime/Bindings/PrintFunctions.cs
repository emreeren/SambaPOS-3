using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.Types;

namespace Fluentscript.Lib.Runtime.Bindings
{
    /// <summary>
    /// Bindings for print functions.
    /// </summary>
    public class PrintFunctions : LanguageBinding
    {
        public PrintFunctions()
        {
            this.ExportedFunctions = new List<string>();
            this.Namespace = "";
            this.ExportedFunctions.Add("print");
            this.ExportedFunctions.Add("println");
        }


        /// <summary>
        /// Prints to the console.
        /// </summary>
        /// <param name="exp"></param>
        public void Print(FunctionCallExpr exp)
        {
            var settings = this.Ctx.Settings;
            if (!settings.EnablePrinting) 
                return; 

            string message = BuildMessage(exp.ParamList);
            if (settings.EnablePrinting) 
                Console.Write(message);
        }


        /// <summary>
        /// Prints a line to the console.
        /// </summary>
        /// <param name="exp"></param>
        public void PrintLn(FunctionCallExpr exp)
        {
            var settings = this.Ctx.Settings;
            if (!settings.EnablePrinting)
                return;

            string message = BuildMessage(exp.ParamList);
            if (settings.EnablePrinting)
                Console.WriteLine(message);
        }


        /// <summary>
        /// Logs a call to the console.
        /// </summary>
        /// <param name="exp"></param>
        public void Log(FunctionCallExpr exp)
        {
            var settings = this.Ctx.Settings;
            if (!settings.EnablePrinting)
                return; 
            
            var funcname = exp.ToQualifiedName();
            var severity = funcname.Substring(funcname.IndexOf(".") + 1);
            var message = BuildMessage(exp.ParamList);
            Console.WriteLine(severity.ToUpper() + " : " + message);
        }


        /// <summary>
        /// Builds a single message from multiple arguments
        /// If there are 2 or more arguments, the 1st is a format, then rest are the args to the format.
        /// </summary>
        /// <param name="paramList">The list of parameters</param>
        /// <returns></returns>
        public static string BuildMessage(List<object> paramList)
        {
            var val = string.Empty;
            if (paramList != null && paramList.Count > 0)
            {
                // Check for 2 arguments which reflects formatting the printing.
                var hasFormat = paramList.Count > 1;
                if (hasFormat)
                {
                    var format = GetVal(paramList[0]);
                    var args = paramList.GetRange(1, paramList.Count - 1);
                    val = string.Format(format, args.ToArray());
                }
                else
                {
                    val = GetVal(paramList[0]);
                }
            }
            return val;
        }


        private static string GetVal(object val)
        {
            var text = "";
            if (val is LObject)
                text = ((LObject)val).GetValue().ToString();
            else
                text = val.ToString();
            return text;
        }
    }
}
