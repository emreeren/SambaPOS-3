using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Localization;
using Samba.Presentation.Common.Services;

namespace Samba.Presentation.Common
{
    public abstract class WidgetViewModel : ObservableObject, IDiagram
    {
        protected readonly Widget _model;
        private bool _isEnabled;

        protected WidgetViewModel(Widget model)
        {
            _model = model;
        }

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

        public int X
        {
            get { return _model.XLocation; }
            set { _model.XLocation = value; RaisePropertyChanged(() => X); }
        }

        public int Y
        {
            get { return _model.YLocation; }
            set { _model.YLocation = value; RaisePropertyChanged(() => Y); }
        }

        [LocalizedDisplayName(ResourceStrings.Height)]
        public int Height
        {
            get { return _model.Height; }
            set { _model.Height = value; RaisePropertyChanged(() => Height); }
        }

        [LocalizedDisplayName(ResourceStrings.Width)]
        public int Width
        {
            get { return _model.Width; }
            set { _model.Width = value; RaisePropertyChanged(() => Width); }
        }

        [Browsable(false)]
        public Transform RenderTransform
        {
            get { return new RotateTransform(_model.Angle); }
            set { _model.Angle = ((RotateTransform)value).Angle; }
        }

        [LocalizedDisplayName(ResourceStrings.Angle)]
        public double Angle
        {
            get { return _model.Angle; }
            set
            {
                _model.Angle = value;
                RaisePropertyChanged(() => Angle);
                RaisePropertyChanged(() => RenderTransform);
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

        [LocalizedDisplayName(ResourceStrings.AutoRefresh)]
        public bool AutoRefresh
        {
            get { return Model.AutoRefresh; }
            set { Model.AutoRefresh = value; RaisePropertyChanged(() => AutoRefresh); }
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

        public abstract void Refresh();

    }
}