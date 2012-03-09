namespace Samba.Infrastructure.Data
{
    public abstract class SpecificationValidator<T> where T : class
    {
        public abstract string GetErrorMessage(T model);
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