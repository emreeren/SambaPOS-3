using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.SelfHost;
using System.Windows.Forms;
using Samba.Presentation.Services.Common;

namespace Samba.ApiServer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MefBootstrapper.ComposeParts();
            var creationService = new DataCreationService();
            creationService.CreateData();

            var config = new HttpSelfHostConfiguration("http://localhost:8080");

            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional });

            using (var server = new HttpSelfHostServer(config))
            {
                server.Configuration.DependencyResolver = new MefDependencyResolver(MefBootstrapper.Container);
                server.OpenAsync().Wait();
                Application.Run(new FrmMain());
            }
        }
    }
}