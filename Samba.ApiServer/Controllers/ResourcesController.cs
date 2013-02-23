using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Web.Http;
using Samba.Domain.Models.Entities;
using Samba.Persistance.DaoClasses;

namespace Samba.ApiServer.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ResourcesController : ApiController
    {
        private readonly IEntityDao _resourceDao;
        
        [ImportingConstructor]
        public ResourcesController(IEntityDao resourceDao)
        {
            _resourceDao = resourceDao;
        }

        public IEnumerable<Entity> GetAllResources()
        {
            return _resourceDao.GetEntitiesByState("Available", 2);
        }

        public Entity GetResourceById(int id)
        {
            var product = _resourceDao.GetEntityById(id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public IEnumerable<Entity> GetResourcesByCategory(string category)
        {
            return _resourceDao.FindEntities(null, category, "");
        }
    }
}
