namespace Samba.Persistance.Data.Specification
{
    public abstract class CompositeSpecification<TEntity> : Specification<TEntity> where TEntity : class
    {
        public abstract ISpecification<TEntity> LeftSideSpecification { get; }
        public abstract ISpecification<TEntity> RightSideSpecification { get; }
    }
}
