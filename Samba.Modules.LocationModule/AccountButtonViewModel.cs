using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Localization;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.LocationModule
{
    public class AccountButtonViewModel : ObservableObject, IDiagram
    {
        private readonly bool _isTicketSelected;
        private readonly bool _userPermittedToMerge;
        private readonly ICommand _actionCommand;

        public AccountButtonViewModel(ResourceScreenItem model, ResourceScreen screen)
            : this(model, screen, null, false, true, null)
        {

        }

        public AccountButtonViewModel(ResourceScreenItem model, ResourceScreen screen,
            ICommand actionCommand, bool isTicketSelected, bool userPermittedToMerge, ResourceState state)
        {
            _actionCommand = actionCommand;
            _screen = screen;
            _isTicketSelected = isTicketSelected;
            _userPermittedToMerge = userPermittedToMerge;
            AccountState = state;
            Model = model;
        }

        public ResourceState AccountState { get; set; }

        private readonly ResourceScreen _screen;
        private ResourceScreenItem _model;

        [Browsable(false)]
        public ResourceScreenItem Model
        {
            get { return _model; }
            set
            {
                _model = value;
                UpdateButtonColor();
            }
        }

        [LocalizedDisplayName(ResourceStrings.Location)]
        public string Name { get { return Model.Name; } }

        private string _buttonColor;
        [Browsable(false)]
        public string ButtonColor
        {
            get { return _buttonColor; }
            set
            {
                if (_buttonColor != value)
                {
                    _buttonColor = value;
                    RaisePropertyChanged(() => ButtonColor);
                }
            }
        }

        [Browsable(false)]
        public double ButtonHeight { get { return _screen.ButtonHeight > 0 ? _screen.ButtonHeight : double.NaN; } }

        [Browsable(false)]
        public ICommand Command
        {
            get { return _actionCommand; }
        }

        private bool _isEnabled;
        [Browsable(false)]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; RaisePropertyChanged(() => IsEnabled); }
        }

        [Browsable(false)]
        public string Caption
        {
            get { return Model.Name; }
            set { Model.Name = value; RaisePropertyChanged(() => Caption); }
        }

        public int X
        {
            get { return Model.XLocation; }
            set { Model.XLocation = value; RaisePropertyChanged(() => X); }
        }

        public int Y
        {
            get { return Model.YLocation; }
            set { Model.YLocation = value; RaisePropertyChanged(() => Y); }
        }

        [LocalizedDisplayName(ResourceStrings.Height)]
        public int Height
        {
            get { return Model.Height; }
            set { Model.Height = value; RaisePropertyChanged(() => Height); }
        }

        [LocalizedDisplayName(ResourceStrings.Width)]
        public int Width
        {
            get { return Model.Width; }
            set { Model.Width = value; RaisePropertyChanged(() => Width); }
        }

        [Browsable(false)]
        public Transform RenderTransform
        {
            get { return new RotateTransform(Model.Angle); }
            set { Model.Angle = ((RotateTransform)value).Angle; }
        }

        [LocalizedDisplayName(ResourceStrings.Angle)]
        public double Angle
        {
            get { return Model.Angle; }
            set
            {
                Model.Angle = value;
                RaisePropertyChanged(() => Angle);
                RaisePropertyChanged(() => RenderTransform);
            }
        }

        [LocalizedDisplayName(ResourceStrings.CornerRadius)]
        public CornerRadius CornerRadius
        {
            get { return new CornerRadius(Model.CornerRadius); }
            set { Model.CornerRadius = Convert.ToInt32(value.BottomLeft); RaisePropertyChanged(() => CornerRadius); }
        }

        public void EditProperties()
        {
            InteractionService.UserIntraction.EditProperties(this);
        }

        public void UpdateButtonColor()
        {
            IsEnabled = true;
            if (_isTicketSelected && !_userPermittedToMerge) IsEnabled = false;
            ButtonColor = AccountState != null ? AccountState.Color : "Silver";

            //ButtonColor = Model.TicketId == 0
            //    ? _screen.LocationEmptyColor
            //    : (Model.IsTicketLocked ? _screen.LocationLockedColor : _screen.LocationFullColor);
        }
    }
}
