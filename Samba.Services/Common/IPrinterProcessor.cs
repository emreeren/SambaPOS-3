using System.Collections.Generic;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Common
{
    public interface IPrinterProcessor
    {
        string Name { get; }
        string[] Process(Ticket ticket, IList<Order> orders, string[] formattedLines);
        void EditSettings();
    }
}
