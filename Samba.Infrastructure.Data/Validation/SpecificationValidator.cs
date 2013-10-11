using System;
using Samba.Localization.Properties;

namespace Samba.Infrastructure.Data.Validation
{
    public abstract class SpecificationValidator<T> where T : class
    {
        public abstract string GetErrorMessage(T model);
    }

    public class GenericDeleteValidator<T> : SpecificationValidator<T> where T : class
    {
        private readonly Func<T, bool> _validationFunction;
        private readonly string _modelName;
        private readonly string _entityName;

        public GenericDeleteValidator(Func<T, bool> validationFunction, string modelName, string entityName)
        {
            _validationFunction = validationFunction;
            _modelName = modelName;
            _entityName = entityName;
        }

        public override string GetErrorMessage(T model)
        {
            var result = _validationFunction.Invoke(model);
            return result ? string.Format(Resources.DeleteErrorUsedBy_f, _modelName, _entityName) : "";
        }
    }

    public enum SuggestedOperation
    {
        Break, Refresh, Continue
    }

    public class ConcurrencyCheckResult
    {
        public string ErrorMessage { get; set; }
        public SuggestedOperation SuggestedOperation { get; set; }

        public static ConcurrencyCheckResult Create(SuggestedOperation suggestedOperation, string errorMessage = "")
        {
            return new ConcurrencyCheckResult { ErrorMessage = errorMessage, SuggestedOperation = suggestedOperation };
        }

        public static ConcurrencyCheckResult Break(string errorMessaage)
        {
            return Create(SuggestedOperation.Break, errorMessaage);
        }

        public static ConcurrencyCheckResult Refresh()
        {
            return Create(SuggestedOperation.Refresh);
        }

        public static ConcurrencyCheckResult Continue()
        {
            return Create(SuggestedOperation.Continue);
        }
    }

    public abstract class ConcurrencyValidator<T> where T : class
    {
        public abstract ConcurrencyCheckResult GetErrorMessage(T current, T loaded);
    }
}