using System;
using System.Linq.Expressions;

namespace Samba.Persistance.Data
{
    public interface ISpecification<TEntity>
        where TEntity : class
    {
        Expression<Func<TEntity, bool>> SatisfiedBy();
    }
}
