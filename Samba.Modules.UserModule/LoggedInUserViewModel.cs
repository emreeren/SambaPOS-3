using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Users;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.UserModule
{
    [Export]
    public class LoggedInUserViewModel : ObservableObject
    {
        [ImportingConstructor]
        public LoggedInUserViewModel()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.UserLoggedIn) UserLoggedIn(x.Value);
                if (x.Topic == EventTopicNames.UserLoggedOut) UserLoggedOut(x.Value);
            });
            LoggedInUser = AppServices.CurrentLoggedInUser;

            LogoutUserCommand = new DelegateCommand<User>(x =>
            {
                if (AppServices.CanNavigate())
                {
                    if (AppServices.IsUserPermittedFor(PermissionNames.OpenNavigation))
                    {
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateNavigation);
                    }
                    else
                    {
                        AppServices.CurrentLoggedInUser.PublishEvent(EventTopicNames.UserLoggedOut);
                        AppServices.LogoutUser();
                    }
                }
            });
        }

        public string LoggedInUserName { get { return LoggedInUser != null ? LoggedInUser.Name : ""; } }
        public User LoggedInUser { get; set; }

        public DelegateCommand<User> LogoutUserCommand { get; set; }

        private void UserLoggedIn(User user)
        {
            LoggedInUser = user;
            RaisePropertyChanged(() => LoggedInUserName);
        }

        private void UserLoggedOut(User user)
        {
            LoggedInUser = User.Nobody;
            RaisePropertyChanged(() => LoggedInUserName);
        }
    }
}
