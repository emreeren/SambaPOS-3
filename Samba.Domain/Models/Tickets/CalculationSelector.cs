using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class CalculationSelector : Entity, IOrderable
    {
        public CalculationSelector()
        {
            _calculationTemplates = new List<CalculationTemplate>();
            _calculationSelectorMaps = new List<CalculationSelectorMap>();
        }

        public string ButtonHeader { get; set; }
        public string ButtonColor { get; set; }
        
        public int Order { get; set; }
        public string UserString { get { return Name; } }
       
        private readonly IList<CalculationTemplate> _calculationTemplates;
        public virtual IList<CalculationTemplate> CalculationTemplates
        {
            get { return _calculationTemplates; }
        }

        private readonly IList<CalculationSelectorMap> _calculationSelectorMaps;
        public virtual IList<CalculationSelectorMap> CalculationSelectorMaps
        {
            get { return _calculationSelectorMaps; }
        }

        public CalculationSelectorMap AddCalculationSelectorMap()
        {
            var map = new CalculationSelectorMap();
            CalculationSelectorMaps.Add(map);
            return map;
        }
    }
}
