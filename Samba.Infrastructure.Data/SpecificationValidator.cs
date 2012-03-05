namespace Samba.Infrastructure.Data
{
    public abstract class SpecificationValidator<T> where T : class
    {
        public abstract string GetErrorMessage(T model);
    }

    public abstract class ConcurrencyValidator<T> where T : class
    {
        public abstract string GetErrorMessage(T current, T loaded);
    }
}