using System;
using System.Printing;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Settings;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    public abstract class AbstractPrintJob
    {
        protected AbstractPrintJob(Printer printer)
        {
            Printer = printer;
        }

        public Printer Printer { get; set; }

        public abstract void DoPrint(string[] lines);
        public abstract void DoPrint(FlowDocument document);

        internal static string RemoveTag(string line)
        {
            return line.Contains(">") ? line.Substring(line.IndexOf(">") + 1) : line;
        }

        internal static void PrintFlowDocument(PrintQueue pq, FlowDocument flowDocument)
        {
            if (pq == null) throw new InvalidOperationException("Invalid Printer");
            // Create a XpsDocumentWriter object, open a Windows common print dialog.
            // This methods returns a ref parameter that represents information about the dimensions of the printer media. 
            XpsDocumentWriter docWriter = PrintQueue.CreateXpsDocumentWriter(pq);
            PageImageableArea ia = pq.GetPrintCapabilities().PageImageableArea;
            PrintTicket pt = pq.UserPrintTicket;

            if (ia != null)
            {
                DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                // Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
                paginator.PageSize = new Size((double)pt.PageMediaSize.Width, (double)pt.PageMediaSize.Height);
                Thickness pagePadding = flowDocument.PagePadding;
                flowDocument.PagePadding = new Thickness(
                        Math.Max(ia.OriginWidth, pagePadding.Left),
                        Math.Max(ia.OriginHeight, pagePadding.Top),
                        Math.Max((double)pt.PageMediaSize.Width - (ia.OriginWidth + ia.ExtentWidth), pagePadding.Right),
                        Math.Max((double)pt.PageMediaSize.Height - (ia.OriginHeight + ia.ExtentHeight), pagePadding.Bottom));
                flowDocument.ColumnWidth = double.PositiveInfinity;
                flowDocument.FontFamily = new System.Windows.Media.FontFamily(LocalSettings.PrintFontFamily);
                // Send DocumentPaginator to the printer.
                docWriter.Write(paginator);
            }
        }
    }
}
