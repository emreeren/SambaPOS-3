using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Web.Http;
using Samba.Domain.Models.Resources;
using Samba.Persistance.DaoClasses;

namespace Samba.ApiServer.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ResourcesController : ApiController
    {
        private readonly IResourceDao _resourceDao;
        
        [ImportingConstructor]
        public ResourcesController(IResourceDao resourceDao)
        {
            _resourceDao = resourceDao;
        }

        public IEnumerable<Resource> GetAllResources()
        {
            return _resourceDao.GetResourcesByState("Available", 2);
        }

        public Resource GetResourceById(int id)
        {
            var product = _resourceDao.GetResourceById(id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public IEnumerable<Resource> GetResourcesByCategory(string category)
        {
            return _resourceDao.FindResources(null, category, "");
        }
    }
}
