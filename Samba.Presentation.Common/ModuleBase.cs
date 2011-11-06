using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.ServiceLocation;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Presentation.Common
{
    public abstract class ModuleBase : IModule
    {
        private readonly List<ICategoryCommand> _dashboardCommands = new List<ICategoryCommand>();
        private readonly IDictionary<Type, VisibleViewModelBase> _objectCache = new Dictionary<Type, VisibleViewModelBase>();

        public void Initialize()
        {
            var moduleLifecycleService = ServiceLocator.Current.GetInstance<IModuleInitializationService>();
            moduleLifecycleService.RegisterForStage(OnPreInitialization, ModuleInitializationStage.PreInitialization);
            moduleLifecycleService.RegisterForStage(OnInitialization, ModuleInitializationStage.Initialization);
            moduleLifecycleService.RegisterForStage(OnPostInitialization, ModuleInitializationStage.PostInitialization);
            moduleLifecycleService.RegisterForStage(OnStartUp, ModuleInitializationStage.StartUp);

            EventServiceFactory.EventService.GetEvent<GenericEvent<VisibleViewModelBase>>().Subscribe(
                s =>
                {
                    if (s.Topic == EventTopicNames.ViewClosed)
                    {
                        _objectCache[s.Value.GetType()] = null;
                    }
                });
        }

        protected virtual void OnPreInitialization()
        {
        }

        protected virtual void OnInitialization()
        {
        }

        protected virtual void OnPostInitialization()
        {
            _dashboardCommands.ForEach(x => x.PublishEvent(EventTopicNames.DashboardCommandAdded));
            _dashboardCommands.Clear();
        }

        protected virtual void OnStartUp()
        {
        }

        protected void AddDashboardCommand<TView>(string caption, string category, int order = 0) where TView : VisibleViewModelBase, new()
        {
            _dashboardCommands.Add(new CategoryCommand<TView>(caption, category, OnExecute) { Order = order });
            _objectCache.Add(typeof(TView), null);
        }

        private void OnExecute<TView>(TView obj) where TView : VisibleViewModelBase, new()
        {
            if (_objectCache[typeof(TView)] == null)
            {
                _objectCache[typeof(TView)] = new TView();
            }
            CommonEventPublisher.PublishViewAddedEvent(_objectCache[typeof(TView)]);
        }
    }
}