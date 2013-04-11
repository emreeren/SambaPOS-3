using System;
using System.Linq.Expressions;

namespace Samba.Persistance.Specification
{
    public interface ISpecification<TEntity>
        where TEntity : class
    {
        Expression<Func<TEntity, bool>> SatisfiedBy();
    }
}
