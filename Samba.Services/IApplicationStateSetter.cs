using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Users;

namespace Samba.Services
{
    public interface IApplicationStateSetter
    {
        void SetCurrentLoggedInUser(User user);
        void SetCurrentDepartment(int departmentId);
        void SetCurrentApplicationScreen(AppScreens appScreen);
        void SetSelectedResourceScreen(ResourceScreen resourceScreen);
        void SetApplicationLocked(bool isLocked);
        void ResetWorkPeriods();
    }
}