using System.Collections.Generic;
using Samba.Domain.Foundation;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class MenuItemPropertyGroup : IEntity, IOrderable
    {
        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] LastUpdateTime { get; set; }
        public bool SingleSelection { get; set; }
        public bool MultipleSelection { get; set; }

        public int ColumnCount { get; set; }
        public int ButtonHeight { get; set; }
        public int TerminalColumnCount { get; set; }
        public int TerminalButtonHeight { get; set; }

        public bool CalculateWithParentPrice { get; set; }

        private IList<MenuItemProperty> _properties;
        public virtual IList<MenuItemProperty> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }
        
        public MenuItemPropertyGroup()
        {
            _properties = new List<MenuItemProperty>();
            ColumnCount = 5;
            ButtonHeight = 65;
            TerminalColumnCount = 4;
            TerminalButtonHeight = 35;
        }

        public MenuItemProperty AddProperty(string name, decimal price, string defaultCurrency)
        {
            var prp = new MenuItemProperty { Name = name, Price = new Price(price, defaultCurrency) };
            Properties.Add(prp);
            return prp;
        }
    }
}
