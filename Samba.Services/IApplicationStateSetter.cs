using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Users;

namespace Samba.Services
{
    public interface IApplicationStateSetter
    {
        void SetCurrentLoggedInUser(User user);
        void SetCurrentDepartment(int departmentId);
        void SetCurrentApplicationScreen(AppScreens appScreen);
        void SetSelectedLocationScreen(LocationScreen locationScreen);
        void SetApplicationLocked(bool isLocked);
        void ResetWorkPeriods();
    }
}