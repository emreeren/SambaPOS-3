using System.Collections.Generic;

namespace Samba.Infrastructure.Data
{
    public class RepositoryBase<TModel> : IRepository<TModel> where TModel : class,IEntityClass
    {
        private readonly IWorkspace _workspace;

        public RepositoryBase(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        public void Add(TModel item)
        {
            _workspace.Add(item);
        }

        public void Delete(TModel item)
        {
            _workspace.Delete(item);
        }

        public IEnumerable<TModel> GetAll()
        {
            return _workspace.All<TModel>();
        }

        public int GetCount()
        {
            return _workspace.Count<TModel>();
        }

        public void SaveChanges()
        {
            _workspace.CommitChanges();
        }

        public TModel GetById(int id)
        {
            return _workspace.Single<TModel>(x => x.Id == id);
        }

        public TModel GetByName(string name)
        {
            return _workspace.Single<TModel>(x => x.Name.ToLower() == name.ToLower());
        }
    }
}