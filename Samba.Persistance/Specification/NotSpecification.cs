using System;
using System.Linq;
using System.Linq.Expressions;

namespace Samba.Persistance.Specification
{
    public sealed class NotSpecification<TEntity> : Specification<TEntity> where TEntity : class
    {
        readonly Expression<Func<TEntity, bool>> _originalCriteria;

        public NotSpecification(ISpecification<TEntity> originalSpecification)
        {
            if (originalSpecification == null)
                throw new ArgumentNullException("originalSpecification");

            _originalCriteria = originalSpecification.SatisfiedBy();
        }

        public NotSpecification(Expression<Func<TEntity, bool>> originalSpecification)
        {
            if (originalSpecification == null)
                throw new ArgumentNullException("originalSpecification");

            _originalCriteria = originalSpecification;
        }

        public override Expression<Func<TEntity, bool>> SatisfiedBy()
        {
            return Expression.Lambda<Func<TEntity, bool>>(Expression.Not(_originalCriteria.Body),
                                                         _originalCriteria.Parameters.Single());
        }
    }
}
