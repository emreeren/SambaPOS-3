using System.Collections.Generic;

namespace Samba.Infrastructure.Data
{
    public interface IRepository<TModel> where TModel : IEntity
    {
        void Add(TModel item);
        void Delete(TModel item);
        IEnumerable<TModel> GetAll();

        int GetCount();
        void SaveChanges();
        TModel GetById(int id);
        TModel GetByName(string name);
    }
}