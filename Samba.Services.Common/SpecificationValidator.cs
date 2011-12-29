namespace Samba.Services.Common
{
    public abstract class SpecificationValidator<T> where T : class
    {
        public abstract string GetErrorMessage(T model);
    }
}