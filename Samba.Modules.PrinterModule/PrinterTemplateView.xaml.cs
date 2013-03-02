using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Samba.Modules.PrinterModule
{
    /// <summary>
    /// Interaction logic for PrinterTemplateView.xaml
    /// </summary>
    public partial class PrinterTemplateView : UserControl
    {
        public PrinterTemplateView()
        {
            InitializeComponent();
            Loaded += PrinterTemplateView_Loaded;
        }

        void PrinterTemplateView_Loaded(object sender, RoutedEventArgs e)
        {
            var content = DataContext as PrinterTemplateViewModel;
            if (content != null)
            {
                var doc = content.Descriptions;
                HelpDocument.Blocks.Clear();
                var p = new Paragraph();
                p.Inlines.Add(new Bold(new Run("Printer Template Token Documentation")));
                p.Inlines.Add(new LineBreak());

                foreach (var value in doc)
                {
                    if (string.IsNullOrEmpty(value.Value))
                        p.Inlines.Add(new LineBreak());
                    p.Inlines.Add(new Bold(new Run(value.Key)));
                    p.Inlines.Add(new Run(" " + value.Value));
                    p.Inlines.Add(new LineBreak());
                }

                HelpDocument.Blocks.Add(p);
            }

        }
    }
}
