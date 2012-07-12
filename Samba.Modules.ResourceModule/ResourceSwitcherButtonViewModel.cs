using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.ResourceModule
{
    public class ResourceSwitcherButtonViewModel : ObservableObject
    {
        public ResourceScreen Model { get; set; }
        private readonly IApplicationState _applicationState;
        private readonly bool _displayActiveScreen;

        public ResourceSwitcherButtonViewModel(ResourceScreen model, IApplicationState applicationState, bool displayActiveScreen)
        {
            Model = model;
            _applicationState = applicationState;
            _displayActiveScreen = displayActiveScreen;
        }

        public string Caption { get { return Model.Name; } }
        public string ButtonColor { get { return Model != _applicationState.SelectedResourceScreen || !_displayActiveScreen ? "Gainsboro" : "Gray"; } }
        public void Refresh() { RaisePropertyChanged(() => ButtonColor); }
    }
}
