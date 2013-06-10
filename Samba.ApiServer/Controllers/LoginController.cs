using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using Samba.ApiServer.Lib;
using Samba.ApiServer.Responses;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Settings;
using Samba.Persistance;

namespace Samba.ApiServer.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginController :
      ApiController
    {
        private readonly IUserDao _UserDao;

        [ImportingConstructor]
        public LoginController(IUserDao userDao)
        {
            _UserDao = userDao;
        }

        //GET =>  http://localhost:8080/api/getToken/{pin}
        public SambaApiLoginResponse GetLogin(string pin)
        {
            // User ret = _UserDao.GetUserByPinCode(pin);
            SettingsObject settings = new SettingsObject();
            SambaApiLoginResponse ret;

            if (!_UserDao.GetIsUserExists(pin))
            {
                //throw new HttpResponseException(HttpStatusCode.Unauthorized);
                ret = new SambaApiLoginResponse(null, null, HttpStatusCode.Unauthorized);
            }
            else
            {
                User user = _UserDao.GetUserByPinCode(pin);
                ret = new SambaApiLoginResponse(new Token(user.Id),
                                                user,
                                                HttpStatusCode.Accepted,
                                                true);
            }

            return ret;
        }
        //public IEnumerable<User> GetAllUsers()
        //{
        //  return Dao.Query<User>();       
        //}
    }
}
