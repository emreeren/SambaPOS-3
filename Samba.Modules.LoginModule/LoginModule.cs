using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Login
{
    [ModuleExport(typeof(LoginModule))]
    public class LoginModule : VisibleModuleBase
    {
        readonly IRegionManager _regionManager;
        private readonly LoginView _loginView;

        [ImportingConstructor]
        public LoginModule(IRegionManager regionManager, LoginView loginView)
            : base(regionManager, AppScreens.LoginScreen)
        {
            _regionManager = regionManager;
            _loginView = loginView;
            SetNavigationCommand("Logout", Resources.Common, "images/bmp.png", 99);
        }

        protected override bool CanNavigate(string arg)
        {
            return true;
        }

        protected override void OnNavigate(string obj)
        {
            AppServices.CurrentLoggedInUser.PublishEvent(EventTopicNames.UserLoggedOut);
            AppServices.LogoutUser();
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion("MainRegion", typeof(LoginView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.UserLoggedOut) 
                        Activate();
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<string>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.PinSubmitted) 
                        PinEntered(x.Value);
                });
        }

        public void PinEntered(string pin)
        {
            var u = AppServices.LoginUser(pin);
            if (u != User.Nobody)
                u.PublishEvent(EventTopicNames.UserLoggedIn);
        }

        public override object GetVisibleView()
        {
            return _loginView;
        }
    }
}
