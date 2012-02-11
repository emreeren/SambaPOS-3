using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    [DataContract]
    public class CustomDataValue
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
        public AccountCustomField CustomField { get; set; }
    }

    public class AccountCustomDataViewModel : ObservableObject
    {
        public Account Model { get; set; }

        public AccountCustomDataViewModel(Account model)
        {
            Model = model;
        }

        private ObservableCollection<CustomDataValue> _customData;
        public ObservableCollection<CustomDataValue> CustomData
        {
            get { return _customData ?? (_customData = GetCustomData(Model.CustomData)); }
        }

        private ObservableCollection<CustomDataValue> GetCustomData(string customData)
        {
            var data = new ObservableCollection<CustomDataValue>();
            try
            {
                if (!string.IsNullOrWhiteSpace(customData))
                    data = JsonHelper.Deserialize<ObservableCollection<CustomDataValue>>(customData) ??
                    new ObservableCollection<CustomDataValue>();
            }
            finally
            {
                GenerateFields(data);
            }
            return data;
        }

        private void GenerateFields(ICollection<CustomDataValue> data)
        {
            if (Model.AccountTemplate != null)
            {
                data.Where(x => !Model.AccountTemplate.AccountCustomFields.Any(y => y.Name == x.Name)).ToList().ForEach(x => data.Remove(x));

                foreach (var cf in Model.AccountTemplate.AccountCustomFields)
                {
                    var customField = cf;
                    var d = data.FirstOrDefault(x => x.Name == customField.Name);
                    if (d == null) data.Add(new CustomDataValue { Name = cf.Name, CustomField = cf });
                    else d.CustomField = cf;
                }
            }
        }

        public void Update()
        {
            Model.CustomData = JsonHelper.Serialize(_customData);
        }
    }
}
