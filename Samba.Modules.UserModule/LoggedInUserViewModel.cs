using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Users;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.UserModule
{
    [Export]
    public class LoggedInUserViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public LoggedInUserViewModel(IApplicationState applicationState, IUserService userService)
        {
            _userService = userService;
            _applicationState = applicationState;

            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.UserLoggedIn) UserLoggedIn(x.Value);
                if (x.Topic == EventTopicNames.UserLoggedOut) UserLoggedOut(x.Value);
            });

            LoggedInUser = applicationState.CurrentLoggedInUser;

            LogoutUserCommand = new DelegateCommand<User>(x =>
            {
                if (!_applicationState.IsLocked)
                {
                    if (_userService.IsUserPermittedFor(PermissionNames.OpenNavigation))
                    {
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateNavigation);
                    }
                    else
                    {
                        _userService.LogoutUser();
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
