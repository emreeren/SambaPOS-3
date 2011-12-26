using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Locations;
using Samba.Persistance.Data.Specification;

namespace Samba.Services.Implementations.LocationModule
{
    public static class LocationSpecifications
    {
        public static Specification<Location> LocationByName(string name)
        {
            return new DirectSpecification<Location>(x => x.Name.ToLower() == name.ToLower());
        }

        public static Specification<Location> LocationById(int id)
        {
            return new DirectSpecification<Location>(x => x.Id == id);
        }

        public static Specification<Location> LocationCanSave(string name, int id)
        {
            Specification<Location> spec = new TrueSpecification<Location>();
            spec &= LocationByName(name);
            spec &= LocationById(id);
            return spec;
        }
    }
}
