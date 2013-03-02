﻿using Samba.Infrastructure.Data;

namespace Samba.Persistance.Data
{
    public class NonDuplicateSaveValidator<T> : SpecificationValidator<T> where T : class, IEntityClass
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