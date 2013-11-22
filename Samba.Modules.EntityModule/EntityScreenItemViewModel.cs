using System.Windows.Input;
using Samba.Domain.Models.Entities;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.EntityModule
{
    public class EntityScreenItemViewModel : ObservableObject
    {
        private readonly bool _isTicketSelected;
        private readonly bool _userPermittedToMerge;
        private readonly ICacheService _cacheService;
        private readonly ICommand _actionCommand;

        public EntityScreenItemViewModel(ICacheService cacheService, EntityScreenItem model, EntityScreen screen,
            ICommand actionCommand, bool isTicketSelected, bool userPermittedToMerge)
        {
            _cacheService = cacheService;
            _actionCommand = actionCommand;
            _screen = screen;
            _isTicketSelected = isTicketSelected;
            _userPermittedToMerge = userPermittedToMerge;
            Model = model;
        }

        public string EntityState { get { return Model.EntityState; } }

        private readonly EntityScreen _screen;

        private EntityScreenItem _model;
        public EntityScreenItem Model
        {
            get { return _model; }
            set
            {
                _model = value;
                UpdateButtonColor();
            }
        }

        public string Name { get { return Model.Name; } }

        private string _buttonColor;
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

        public int FontSize
        {
            get { return _screen.FontSize; }
        }

        public double ButtonHeight { get { return _screen.ButtonHeight > 0 ? _screen.ButtonHeight : double.NaN; } }

        public ICommand Command
        {
            get { return _actionCommand; }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; RaisePropertyChanged(() => IsEnabled); }
        }

        public void UpdateButtonColor()
        {
            IsEnabled = true;
            ButtonColor = EntityState != null ? _cacheService.GetStateColor(EntityState) : "Gainsboro";
        }
    }
}
