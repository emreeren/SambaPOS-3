using System;
using System.Linq.Expressions;

namespace Samba.Persistance.Specification
{
    public sealed class AndSpecification<T> : CompositeSpecification<T> where T : class
    {
        private readonly ISpecification<T> _rightSideSpecification;
        private readonly ISpecification<T> _leftSideSpecification;

        public AndSpecification(ISpecification<T> leftSide, ISpecification<T> rightSide)
        {
            if (leftSide == null)
                throw new ArgumentNullException("leftSide");

            if (rightSide == null)
                throw new ArgumentNullException("rightSide");

            _leftSideSpecification = leftSide;
            _rightSideSpecification = rightSide;
        }

        public override ISpecification<T> LeftSideSpecification
        {
            get { return _leftSideSpecification; }
        }

        public override ISpecification<T> RightSideSpecification
        {
            get { return _rightSideSpecification; }
        }

        public override Expression<Func<T, bool>> SatisfiedBy()
        {
            Expression<Func<T, bool>> left = _leftSideSpecification.SatisfiedBy();
            Expression<Func<T, bool>> right = _rightSideSpecification.SatisfiedBy();

            return (left.And(right));
        }
    }
}
