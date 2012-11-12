using Samba.Infrastructure.Data;
using Samba.Persistance.Data;

namespace Samba.Persistance
{
    public class NonDuplicateSaveValidator<T> : SpecificationValidator<T> where T : class, IEntity
    {
        private readonly string _errorMessage;

        public NonDuplicateSaveValidator(string errorMessage)
        {
            _errorMessage = errorMessage;
        }

        public override string GetErrorMessage(T model)
        {
            return EntitySpecifications.EntityDuplicates(model).Exists() ? _errorMessage : "";
        }
    }
}