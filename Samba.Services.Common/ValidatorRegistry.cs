namespace Samba.Services.Common
{
    public static class ValidatorRegistry
    {
        private static readonly DictionaryByType SaveValidators = new DictionaryByType();
        private static readonly DictionaryByType DeleteValidators = new DictionaryByType();

        public static void RegisterSaveValidator<T>(SpecificationValidator<T> validator) where T : class
        {
            SaveValidators.Add(validator);
        }

        public static void RegisterDeleteValidator<T>(SpecificationValidator<T> validator) where T : class
        {
            DeleteValidators.Add(validator);
        }

        public static string GetSaveErrorMessage<T>(T model) where T : class
        {
            SpecificationValidator<T> validator;
            SaveValidators.TryGet(out validator);
            return validator.GetErrorMessage(model);
        }

        public static string GetDeleteErrorMessage<T>(T model) where T : class
        {
            SpecificationValidator<T> validator;
            DeleteValidators.TryGet(out validator);
            return validator.GetErrorMessage(model);
        }
    }
}