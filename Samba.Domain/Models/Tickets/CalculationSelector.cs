using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class CalculationSelector : EntityClass, IOrderable
    {
        public CalculationSelector()
        {
            _calculationTypes = new List<CalculationType>();
            _calculationSelectorMaps = new List<CalculationSelectorMap>();
            FontSize = 30;
        }

        public string ButtonHeader { get; set; }
        public string ButtonColor { get; set; }
        public int FontSize { get; set; }

        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }

        private IList<CalculationType> _calculationTypes;
        public virtual IList<CalculationType> CalculationTypes
        {
            get { return _calculationTypes; }
            set { _calculationTypes = value; }
        }

        private IList<CalculationSelectorMap> _calculationSelectorMaps;
        public virtual IList<CalculationSelectorMap> CalculationSelectorMaps
        {
            get { return _calculationSelectorMaps; }
            set { _calculationSelectorMaps = value; }
        }

        public CalculationSelectorMap AddCalculationSelectorMap()
        {
            var map = new CalculationSelectorMap();
            CalculationSelectorMaps.Add(map);
            return map;
        }
    }
}
