using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Windows.Forms;
using Samba.ApiServer.Lib;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Services.Common;
using Samba.Presentation.Services.Common.DataGeneration;

namespace Samba.ApiServer
{
    internal static class Program
    {
        static Program()
        {
            if (LocalSettings.ApiHost == null)
            {
                LocalSettings.ApiHost = "localhost";
            }
            if (LocalSettings.ApiPort == null)
            {
                LocalSettings.ApiPort = "8080";
            }
            if (LocalSettings.TokenLifeTime.Ticks == 0)
            {
                LocalSettings.TokenLifeTime = new TimeSpan(0, 30, 0);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.ApplicationExit += Token.ApplicationExit;
            Application.SetCompatibleTextRenderingDefault(false);
            MefBootstrapper.ComposeParts();
            var creationService = new DataCreationService();
            creationService.CreateData();

            var apiHost = LocalSettings.ApiHost;
            var apiPort = LocalSettings.ApiPort;
            var httpHost = string.Format("http://{0}:{1}", apiHost, apiPort);

            var config = new HttpSelfHostConfiguration(httpHost);

            //GET =>  http://localhost:8080/api/getToken/{pin}
            config.Routes.MapHttpRoute("LoginRoute", "api/getToken/{pin}", new
                                                                           {
                                                                               controller = "Login"
                                                                           });
            //GET =>  http://localhost:8080/api/{token}/{controller}/{id}
            config.Routes.MapHttpRoute("API Default", "api/{token}/{controller}/{id}",
                                       new
                                       {
                                           id = RouteParameter.Optional
                                       });

            using (var server = new HttpSelfHostServer(config))
            {
                server.Configuration.DependencyResolver = new MefDependencyResolver(MefBootstrapper.Container);
                server.OpenAsync().Wait();

                if (LocalSettings.TokenLifeTime.Ticks > 0)
                {
                    var tokenGarbageTimer = new Timer
                                                {
                                                    Interval = (int)new TimeSpan(0, 1, 0).TotalMilliseconds
                                                };
                    tokenGarbageTimer.Tick += Token.CollectGarbage;
                    tokenGarbageTimer.Start();
                }
                Application.Run(new FrmMain());
            }
        }
    }
}