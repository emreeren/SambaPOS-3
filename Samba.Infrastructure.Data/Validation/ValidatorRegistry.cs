using System;

namespace Samba.Infrastructure.Data.Validation
{
    public static class ValidatorRegistry
    {
        private static readonly DictionaryByType SaveValidators = new DictionaryByType();
        private static readonly DictionaryByType DeleteValidators = new DictionaryByType();
        private static readonly DictionaryByType ConcurrencyValidators = new DictionaryByType();

        public static void RegisterSaveValidator<T>(SpecificationValidator<T> validator) where T : class
        {
            SaveValidators.Add(validator);
        }

        public static void RegisterDeleteValidator<T>(SpecificationValidator<T> validator) where T : class
        {
            DeleteValidators.Add(validator);
        }

        public static void RegisterDeleteValidator<T>(Func<T, bool> validationFunction, string modelName, string entityName) where T : class
        {
            SpecificationValidator<T> validator = new GenericDeleteValidator<T>(validationFunction, modelName, entityName);
            DeleteValidators.Add(validator);
        }

        public static void RegisterConcurrencyValidator<T>(ConcurrencyValidator<T> validator) where T : class
        {
            ConcurrencyValidators.Add(validator);
        }

        public static string GetSaveErrorMessage<T>(T model) where T : class
        {
            SpecificationValidator<T> validator;
            SaveValidators.TryGet(out validator);
            return validator != null ? validator.GetErrorMessage(model) : "";
        }

        public static string GetDeleteErrorMessage<T>(T model) where T : class
        {
            SpecificationValidator<T> validator;
            DeleteValidators.TryGet(out validator);
            return validator != null ? validator.GetErrorMessage(model) : "";
        }

        public static ConcurrencyCheckResult GetConcurrencyErrorMessage<T>(T current, T loaded) where T : class
        {
            ConcurrencyValidator<T> validator;
            ConcurrencyValidators.TryGet(out validator);
            return validator != null ? validator.GetErrorMessage(current, loaded) : ConcurrencyCheckResult.Continue();
        }
    }
}