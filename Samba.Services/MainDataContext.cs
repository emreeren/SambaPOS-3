using System.Collections.Generic;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Menus;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public class MainDataContext
    {
        public string NumeratorValue { get; set; }


        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = Dao.Query<AppRule>(x => x.Actions)); } }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = Dao.Query<AppAction>()); } }

        private IEnumerable<TaxTemplate> _taxTemplates;
        public IEnumerable<TaxTemplate> TaxTemplates
        {
            get { return _taxTemplates ?? (_taxTemplates = Dao.Query<TaxTemplate>()); }
        }

        private IEnumerable<ServiceTemplate> _serviceTemplates;
        public IEnumerable<ServiceTemplate> ServiceTemplates
        {
            get { return _serviceTemplates ?? (_serviceTemplates = Dao.Query<ServiceTemplate>()); }
        }

        public void ResetCache()
        {
            //var selectedDepartment = DepartmentService.CurrentDepartment != null ? DepartmentService.CurrentDepartment.Id : 0;
            //var selectedLocationScreen = LocationService.SelectedLocationScreen != null ? LocationService.SelectedLocationScreen.Id : 0;

            //LocationService.SelectedLocationScreen = null;
            //DepartmentService.SelectDepartment(null);
            //IService.
            //_rules = null;
            //_actions = null;
            //_taxTemplates = null;
            //_serviceTemplates = null;

            //DepartmentService.SelectDepartment(selectedDepartment);

            //if (selectedDepartment > 0 && Departments.Count(x => x.Id == selectedDepartment) > 0)
            //{
            //    SelectedDepartment = Departments.Single(x => x.Id == selectedDepartment);
            //    if (selectedLocationScreen > 0 && SelectedDepartment.PosLocationScreens.Count(x => x.Id == selectedLocationScreen) > 0)
            //        SelectedLocationScreen = SelectedDepartment.PosLocationScreens.Single(x => x.Id == selectedLocationScreen);
            //}
        }
        
        public TaxTemplate GetTaxTemplate(int menuItemId)
        {
            //todo fix
            return new TaxTemplate();
            //return AppServices.DataAccessService.GetMenuItem(menuItemId).TaxTemplate;
        }
    }
}
