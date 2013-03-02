using System.Collections.ObjectModel;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class ModelListViewModelBase : ViewModelBase
    {
        public ObservableCollection<VisibleViewModelBase> Views { get; set; }

        protected ModelListViewModelBase()
        {
            Views = new ObservableCollection<VisibleViewModelBase>();
            AttachEvents();
        }

        private void AttachEvents()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<VisibleViewModelBase>>().Subscribe(
                s =>
                {
                    if (s.Topic == EventTopicNames.ViewClosed)
                    {
                        if (s.Value != null)
                        {
                            if (Views.Contains(s.Value))
                                Views.Remove(s.Value);
                            if (s.Value.CallingView != null)
                                SetActiveView(Views, s.Value.CallingView);
                            s.Value.OnClosed();
                            s.Value.CallingView = null;
                            s.Value.Dispose();
                        }
                    }

                    if (s.Topic == EventTopicNames.ViewAdded && s.Value != null)
                    {
                        s.Value.CallingView = GetActiveView(Views);
                        if (!Views.Contains(s.Value))
                            Views.Add(s.Value);
                        SetActiveView(Views, s.Value);
                        s.Value.OnShown();
                    }
                });
        }
    }
}
