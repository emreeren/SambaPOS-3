using System.Collections.Generic;
using System.Runtime.Serialization;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Accounts
{
    public class AccountScreen : EntityClass, IOrderable
    {
        public AccountScreen()
        {
            _accountScreenValues = new List<AccountScreenValue>();
        }

        public int Filter { get; set; }
        public bool DisplayAsTree { get; set; }
        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }
        public string AutomationCommandMapData { get; set; }

        private IList<AccountScreenValue> _accountScreenValues;
        public virtual IList<AccountScreenValue> AccountScreenValues
        {
            get { return _accountScreenValues; }
            set { _accountScreenValues = value; }
        }

        private IEnumerable<AccountScreenAutmationCommandMap> _autmationCommandMaps;
        public IEnumerable<AccountScreenAutmationCommandMap> AutmationCommandMaps
        {
            get { return _autmationCommandMaps ?? (_autmationCommandMaps = GetAutomationCommandMaps()); }
        }

        private IEnumerable<AccountScreenAutmationCommandMap> GetAutomationCommandMaps()
        {
            return JsonHelper.Deserialize<List<AccountScreenAutmationCommandMap>>(AutomationCommandMapData);
        }

        public void SetAutomationCommandMaps(IEnumerable<AccountScreenAutmationCommandMap> autmationCommandMaps)
        {
            AutomationCommandMapData = JsonHelper.Serialize(autmationCommandMaps);
            _autmationCommandMaps = null;
        }
    }

    [DataContract]
    public class AccountScreenAutmationCommandMap : IOrderable
    {
        [DataMember(Name = "AC")]
        public string AutomationCommandName { get; set; }
        [DataMember(Name = "SO")]
        public int SortOrder { get; set; }
        [DataMember(Name = "VT", EmitDefaultValue = false)]
        public int AutomationCommandValueType { get; set; }

        public string Name { get { return AutomationCommandName; } }
        public string UserString { get { return Name; } }
    }
}
