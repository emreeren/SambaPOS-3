using System;

namespace Samba.Presentation.Services.Common
{
    public class AppScreenChangeData
    {
        public AppScreenChangeData(AppScreens pre, AppScreens next)
        {
            PreviousScreen = pre;
            CurrentScreen = next;
        }
        public AppScreens PreviousScreen { get; set; }
        public AppScreens CurrentScreen { get; set; }
    }
}