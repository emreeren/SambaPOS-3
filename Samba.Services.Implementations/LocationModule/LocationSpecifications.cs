using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Persistance.Data.Specification;

namespace Samba.Services.Implementations.LocationModule
{
    public static class LocationSpecifications
    {
        public static bool LocationScreenContainsLocation(int locationId)
        {
            return true;
        }

        public static Specification<LocationScreen> LocationScreensByLocationId(int locationId)
        {
            return new DirectSpecification<LocationScreen>(x => x.Locations.Any(y => y.Id == locationId));
        }

        public static Specification<Department> DepartmentsByLocationScreenId(int id)
        {
            return new DirectSpecification<Department>(x => x.LocationScreens.Any(y => y.Id == id));
        }
    }

    public static class EasySpec<TSource> where TSource : class
    {
        public static bool Contains(Expression<Func<TSource, bool>> expression)
        {
            return new DirectSpecification<TSource>(expression).Exists();
        }

        public static bool Contains2(Expression<Func<TSource, bool>> expression)
        {
            return Dao.Exists(expression);
        }
    }
}
