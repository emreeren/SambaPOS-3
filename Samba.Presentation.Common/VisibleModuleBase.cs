using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Common
{
    public abstract class VisibleModuleBase : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly AppScreens _appScreen;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private ICategoryCommand _navigationCommand;

        protected VisibleModuleBase(IRegionManager regionManager, AppScreens appScreen)
        {
            _applicationStateSetter = ServiceLocator.Current.GetInstance<IApplicationStateSetter>();
            _regionManager = regionManager;
            _appScreen = appScreen;
        }

        public void Activate()
        {
            _applicationStateSetter.SetCurrentApplicationScreen(_appScreen);
            _regionManager.ActivateRegion(RegionNames.MainRegion, GetVisibleView());
        }

        protected void SetNavigationCommand(string caption, string category, string image, int order = 0)
        {
            _navigationCommand = new CategoryCommand<string>(caption, category, image, OnNavigate, CanNavigate) { Order = order };
        }

        protected virtual bool CanNavigate(string arg)
        {
            return true;
        }

        protected virtual void OnNavigate(string obj)
        {
            Activate();
        }

        public abstract object GetVisibleView();

        protected sealed override void OnPostInitialization()
        {
            if (_navigationCommand != null)
                _navigationCommand.PublishEvent(EventTopicNames.NavigationCommandAdded);
            base.OnPostInitialization();
        }
    }
}
