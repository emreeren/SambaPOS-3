using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Regions;
using Samba.Services;

namespace Samba.Presentation.Common
{
    public abstract class VisibleModuleBase : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly AppScreens _appScreen;
        private ICategoryCommand _navigationCommand;

        protected VisibleModuleBase(IRegionManager regionManager, AppScreens appScreen)
        {
            _regionManager = regionManager;
            _appScreen = appScreen;
        }

        public void Activate()
        {
            AppServices.ActiveAppScreen = _appScreen;
            _regionManager.Regions[RegionNames.MainRegion].Activate(GetVisibleView());
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
