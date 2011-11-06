using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation.Terminal
{
    public class LoggedInUserViewModel : ObservableObject
    {
        public event EventHandler CloseButtonClickedEvent;

        public void InvokeCloseButtonClickedEvent(EventArgs e)
        {
            EventHandler handler = CloseButtonClickedEvent;
            if (handler != null) handler(this, e);
        }

        public string GetLocation()
        {
            var result = "";
            if (DataContext.SelectedTicket != null)
            {
                if (!string.IsNullOrEmpty(DataContext.SelectedTicket.Model.TicketNumber))
                    result += "#" + DataContext.SelectedTicket.Model.TicketNumber + " - ";
                if (!string.IsNullOrEmpty(DataContext.SelectedTicket.Location))
                    result += DataContext.SelectedTicket.Location + " - ";
            }
            return result;
        }

        public string LoggedInUserName { get { return GetLocation() + AppServices.CurrentLoggedInUser.Name; } }
        public ICaptionCommand CloseScreenCommand { get; set; }

        public LoggedInUserViewModel()
        {
            CloseScreenCommand = new CaptionCommand<string>("Close", OnCloseScreen);
        }

        private void OnCloseScreen(string obj)
        {
            RaisePropertyChanged(()=>LoggedInUserName);
            InvokeCloseButtonClickedEvent(EventArgs.Empty);
        }

        public void Refresh()
        {
            RaisePropertyChanged(()=>LoggedInUserName);
        }
    }
}
