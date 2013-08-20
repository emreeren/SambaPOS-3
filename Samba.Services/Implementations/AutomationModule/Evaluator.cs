using System.ComponentModel.Composition;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export]
    internal class Evaluator
    {
        private readonly IExpressionService _expressionService;
        private readonly Preprocessor _preprocessor;

        [ImportingConstructor]
        public Evaluator(IExpressionService expressionService, Preprocessor preprocessor)
        {
            _expressionService = expressionService;
            _preprocessor = preprocessor;
        }

        public bool Evals(string expression, object dataObject, bool defaultValue)
        {
            if (string.IsNullOrEmpty(expression)) return defaultValue;
            expression = _preprocessor.Process(expression, dataObject);
            return _expressionService.Eval("result = " + expression, dataObject, defaultValue);
        }
    }
}