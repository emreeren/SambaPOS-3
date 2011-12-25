namespace Samba.Services
{
    public class MainDataContext
    {
        public string NumeratorValue { get; set; }
        
        public void ResetCache()
        {
            //var selectedDepartment = DepartmentService.CurrentDepartment != null ? DepartmentService.CurrentDepartment.Id : 0;
            //var selectedLocationScreen = LocationService.SelectedLocationScreen != null ? LocationService.SelectedLocationScreen.Id : 0;

            //LocationService.SelectedLocationScreen = null;
            //DepartmentService.SelectDepartment(null);

            //DepartmentService.SelectDepartment(selectedDepartment);

            //if (selectedDepartment > 0 && Departments.Count(x => x.Id == selectedDepartment) > 0)
            //{
            //    SelectedDepartment = Departments.Single(x => x.Id == selectedDepartment);
            //    if (selectedLocationScreen > 0 && SelectedDepartment.PosLocationScreens.Count(x => x.Id == selectedLocationScreen) > 0)
            //        SelectedLocationScreen = SelectedDepartment.PosLocationScreens.Single(x => x.Id == selectedLocationScreen);
            //}
        }
    }
}
