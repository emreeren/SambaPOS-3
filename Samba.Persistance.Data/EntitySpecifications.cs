using Samba.Infrastructure.Data;
using Samba.Persistance.Data.Specification;

namespace Samba.Persistance.Data
{
    public static class EntitySpecifications
    {
        public static Specification<T> EntityByName<T>(string name) where T : class, IEntityClass
        {
            return new DirectSpecification<T>(x => x.Name.ToLower() == name.ToLower());
        }

        public static Specification<T> EntityById<T>(int id) where T : class, IEntityClass
        {
            return new DirectSpecification<T>(x => x.Id == id);
        }

        public static Specification<T> EntityDuplicates<T>(T entity) where T : class, IEntityClass
        {
            Specification<T> spec = new TrueSpecification<T>();
            spec &= EntityByName<T>(entity.Name);
            return spec & !EntityById<T>(entity.Id);
        }
    }
}
