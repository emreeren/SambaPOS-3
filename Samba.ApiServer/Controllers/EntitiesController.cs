using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Web.Http;
using Samba.Domain.Models.Entities;
using Samba.Persistance;

namespace Samba.ApiServer.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class EntitiesController : ApiController
    {
        private readonly IEntityDao _entityDao;
        
        [ImportingConstructor]
        public EntitiesController(IEntityDao entityDao)
        {
            _entityDao = entityDao;
        }

        public IEnumerable<Entity> GetAllEntities()
        {
            return _entityDao.GetEntitiesByState("Available", 2);
        }

        public Entity GetEntityById(int id)
        {
            var product = _entityDao.GetEntityById(id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public IEnumerable<Entity> GetEntitiesByCategory(string category)
        {
            return _entityDao.FindEntities(null, category, "");
        }
    }
}
