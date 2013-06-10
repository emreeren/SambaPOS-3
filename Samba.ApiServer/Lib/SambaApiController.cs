using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;

namespace Samba.ApiServer.Lib
{
    public class SambaApiController : ApiController
    {
        internal void ValidateToken()
        {
            var token = (string)ControllerContext.RouteData.Values["token"];
            if (!Token.ValidateToken(token))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }
    }
}
