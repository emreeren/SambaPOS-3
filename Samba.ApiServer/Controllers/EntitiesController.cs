using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Web.Http;
using Samba.ApiServer.Lib;
using Samba.Domain.Models.Entities;
using Samba.Persistance;

namespace Samba.ApiServer.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class EntitiesController : SambaApiController
    {
        private readonly IEntityDao _entityDao;

        [ImportingConstructor]
        public EntitiesController(IEntityDao entityDao)
        {
            _entityDao = entityDao;
        }

        //GET =>  http://localhost:8080/api/{token}/Entities/
        public IEnumerable<Entity> GetAllEntities(string entityState)
        {
            ValidateToken();
            return _entityDao.GetEntitiesByState(entityState, 2);
        }

        //GET =>  http://localhost:8080/api/{token}/Entities/?category={category}
        public IEnumerable<Entity> GetEntitiesByCategory(string category)
        {
            ValidateToken();
            return _entityDao.FindEntities(null, category, "");
        }

        //GET =>  http://localhost:8080/api/{token}/Entities/{id}
        public Entity GetEntityById(int id)
        {
            ValidateToken();
            Entity product = _entityDao.GetEntityById(id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

    }
}
