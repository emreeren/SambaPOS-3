using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tables
{
    public class TableScreen : IEntity, IOrderable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public int DisplayMode { get; set; }
        public string BackgroundColor { get; set; }
        public string BackgroundImage { get; set; }
        public string TableEmptyColor { get; set; }
        public string TableFullColor { get; set; }
        public string TableLockedColor { get; set; }
        public int PageCount { get; set; }
        public int ColumnCount { get; set; }
        public int ButtonHeight { get; set; }
        public int NumeratorHeight { get; set; }
        public string AlphaButtonValues { get; set; }

        private IList<Table> _tables;
        public virtual IList<Table> Tables
        {
            get { return _tables; }
            set { _tables = value; }
        }

        public string UserString
        {
            get { return Name; }
        }

        public TableScreen()
        {
            _tables = new List<Table>();
            TableEmptyColor = "WhiteSmoke";
            TableFullColor = "Orange";
            TableLockedColor = "Brown";
            BackgroundColor = "Transparent";
            PageCount = 1;
            ButtonHeight = 0;
        }

        public int ItemCountPerPage
        {
            get
            {
                var itemCount = Tables.Count / PageCount;
                if (Tables.Count % PageCount > 0) itemCount++;
                return itemCount;
            }
        }

        public void AddScreenItem(Table choosenValue)
        {
            if (!Tables.Contains(choosenValue))
                Tables.Add(choosenValue);
        }
    }
}
