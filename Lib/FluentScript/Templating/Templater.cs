using System;
using System.Collections.Generic;

namespace Fluentscript.Lib.Templating
{
    /// <summary>
    /// Templating class for the javascript like language.
    /// Syntax similar to jquery templates/python-django templates.
    /// </summary>
    public class Templater
    {
        private static IDictionary<string, Func<ITemplateEngine>> _engines = new Dictionary<string, Func<ITemplateEngine>>();
        

        /// <summary>
        /// Registers a custom template engine.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="engine"></param>
        public static void RegisterEngine(string type, Func<ITemplateEngine> engine)
        {
            _engines[type] = engine;
        }


        /// <summary>
        /// Render using default template engine similar to webforms.
        /// </summary>
        /// <returns></returns>
        public static string Render(string script)
        {
            ITemplateEngine engine = new TemplateEngineDefault(script);
            var finalscript = engine.Render();
            return finalscript;
        }


        /// <summary>
        /// Render using custom template engine.
        /// </summary>
        /// <returns></returns>
        public static string Render(string engineType, string script)
        {
            var templateEngine = _engines[engineType]();
            var finalscript = templateEngine.Render(script);
            return finalscript;
        }
    }
}
