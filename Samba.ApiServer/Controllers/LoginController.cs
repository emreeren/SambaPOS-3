using System.ComponentModel.Composition;
using System.Net;
using System.Web.Http;
using Samba.ApiServer.Lib;
using Samba.ApiServer.Responses;
using Samba.Persistance;

namespace Samba.ApiServer.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginController : ApiController
    {
        private readonly IUserDao _userDao;

        [ImportingConstructor]
        public LoginController(IUserDao userDao)
        {
            _userDao = userDao;
        }

        //GET =>  http://localhost:8080/api/getToken/{pin}
        public SambaApiLoginResponse GetLogin(string pin)
        {
            SambaApiLoginResponse ret;

            if (!_userDao.GetIsUserExists(pin))
            {
                ret = new SambaApiLoginResponse(null, null, HttpStatusCode.Unauthorized);
            }
            else
            {
                var user = _userDao.GetUserByPinCode(pin);
                ret = new SambaApiLoginResponse(new Token(user.Id),
                                                user,
                                                HttpStatusCode.Accepted,
                                                true);
            }

            return ret;
        }
    }
}
