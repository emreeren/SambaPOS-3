using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Persistance.Data
{
    public static class SpecificationDaoExtentions
    {
        public static bool Exists<T>(this ISpecification<T> specification) where T : class
        {
            return Dao.Exists(specification.SatisfiedBy());
        }
    }
}
