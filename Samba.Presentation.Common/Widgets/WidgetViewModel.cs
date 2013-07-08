using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using PropertyTools.DataAnnotations;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Localization;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;

namespace Samba.Presentation.Common.Widgets
{
    public abstract class WidgetViewModel : ObservableObject, IDiagram
    {
        protected readonly Widget _model;
        private readonly IApplicationState _applicationState;
        private bool _isEnabled;
        private bool _disposed;
        private readonly Timer _timer;

        protected WidgetViewModel(Widget model, IApplicationState applicationState)
        {
            _model = model;
            _applicationState = applicationState;
            if (AutoRefreshInterval > 0 && !DesignMode)
            {
                _timer = new Timer(OnTimer, new { }, AutoRefreshInterval * 1000, AutoRefreshInterval * 1000);
            }
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }

        private object _settingsObject;

        [Browsable(false)]
        public object SettingsObject
        {
            get { return _settingsObject ?? (_settingsObject = CreateSettingsObject()); }
        }

        protected abstract object CreateSettingsObject();

        [Browsable(false)]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; RaisePropertyChanged(() => IsEnabled); }
        }

        [Browsable(false)]
        public string CreatorName
        {
            get { return _model.CreatorName; }
            set { _model.CreatorName = value; }
        }

        [Spinnable(1, 10, 0, int.MaxValue)]
        public int X
        {
            get { return _model.XLocation; }
            set { _model.XLocation = value; RaisePropertyChanged(() => X); }
        }

        [Spinnable(1, 10, 0, int.MaxValue)]
        public int Y
        {
            get { return _model.YLocation; }
            set { _model.YLocation = value; RaisePropertyChanged(() => Y); }
        }

        [LocalizedDisplayName(ResourceStrings.Height)]
        [Spinnable(1, 10, 0, int.MaxValue)]
        public int Height
        {
            get { return _model.Height; }
            set { _model.Height = value; RaisePropertyChanged(() => Height); }
        }

        [LocalizedDisplayName(ResourceStrings.Width)]
        [Spinnable(1, 10, 0, int.MaxValue)]
        public int Width
        {
            get { return _model.Width; }
            set { _model.Width = value; RaisePropertyChanged(() => Width); }
        }

        [Browsable(false)]
        public Transform RotateTransform
        {
            get { return new RotateTransform(_model.Angle); }
            set { _model.Angle = ((RotateTransform)value).Angle; }
        }

        [Browsable(false)]
        public Transform ScaleTransform
        {
            get { return new ScaleTransform(Scale, Scale); }
            set { Scale = ((ScaleTransform)value).ScaleX; }
        }

        [LocalizedDisplayName(ResourceStrings.Angle)]
        public double Angle
        {
            get { return _model.Angle; }
            set
            {
                _model.Angle = value;
                RaisePropertyChanged(() => Angle);
                RaisePropertyChanged(() => RotateTransform);
            }
        }

        public double Scale
        {
            get { return _model.Scale > 0 ? _model.Scale : 1; }
            set
            {
                _model.Scale = value;
                RaisePropertyChanged(() => Scale);
                RaisePropertyChanged(() => ScaleTransform);
            }
        }

        [LocalizedDisplayName(ResourceStrings.CornerRadius)]
        public CornerRadius CornerRadius
        {
            get { return new CornerRadius(_model.CornerRadius); }
            set { _model.CornerRadius = Convert.ToInt32(value.BottomLeft); RaisePropertyChanged(() => CornerRadius); }
        }

        [Browsable(false)]
        public Widget Model
        {
            get { return _model; }
        }

        public Widget GetWidget()
        {
            return Model;
        }

        [Browsable(false)]
        public bool DesignMode { get; set; }

        [Browsable(false)]
        public bool IsVisible
        {
            get
            {
                return _applicationState.SelectedEntityScreen != null &&
                       _applicationState.SelectedEntityScreen.Widgets.Contains(Model);
            }
        }

        [LocalizedDisplayName(ResourceStrings.AutoRefresh)]
        public bool AutoRefresh
        {
            get { return Model.AutoRefresh; }
            set { Model.AutoRefresh = value; RaisePropertyChanged(() => AutoRefresh); }
        }

        [LocalizedDisplayName(ResourceStrings.AutoRefreshInterval)]
        public int AutoRefreshInterval
        {
            get { return Model.AutoRefreshInterval; }
            set { Model.AutoRefreshInterval = value; }
        }

        public void EditProperties()
        {
            InteractionService.UserIntraction.EditProperties(this);
        }

        public void EditSettings()
        {
            if (SettingsObject != null)
            {
                BeforeEditSettings();
                InteractionService.UserIntraction.EditProperties(SettingsObject);
            }
        }

        protected virtual void BeforeEditSettings()
        {
            //override if needed
        }

        public void SaveSettings()
        {
            if (SettingsObject != null)
                Model.Properties = JsonHelper.Serialize(SettingsObject);
        }

        private void OnTimer(object state)
        {
            if (!IsVisible) return;
            if (_disposed) return;
            _applicationState.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Refresh));
        }

        protected override void Dispose(bool disposing)
        {
            if (_timer != null)
            {
                _disposed = true;
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }

        public abstract void Refresh();
    }
}