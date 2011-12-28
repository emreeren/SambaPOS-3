using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data.Specification;

namespace Samba.Services.Implementations.LocationModule
{
    public static class LocationSpecifications
    {
        public static Specification<LocationScreen> LocationScreensByLocationId(int locationId)
        {
            return new DirectSpecification<LocationScreen>(x => x.Locations.Any(y => y.Id == locationId));
        }

        public static Specification<Department> DepartmentsByLocationScreenId(int id)
        {
            return new DirectSpecification<Department>(x => x.LocationScreens.Any(y => y.Id == id));
        }
    }
}
