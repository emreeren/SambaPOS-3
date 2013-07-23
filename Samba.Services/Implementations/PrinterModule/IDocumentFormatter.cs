using System;
using Samba.Domain.Models.Settings;

namespace Samba.Services.Implementations.PrinterModule
{
    public interface IDocumentFormatter
    {
        Type ObjectType { get; }
        string[] GetFormattedDocument(object item, PrinterTemplate printerTemplate);
    }
}