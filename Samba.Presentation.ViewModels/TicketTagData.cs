namespace Samba.Presentation.ViewModels
{
    public class TicketTagData
    {
        public string TagName { get; set; }

        private string _tagValue;
        public string TagValue
        {
            get { return _tagValue ?? string.Empty; }
            set { _tagValue = value; }
        }

        public int Action { get; set; }
        public decimal NumericValue { get; set; }
    }
}