using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class OrderTagValue
    {
        public OrderTagValue()
        {
            TagValue = "";
        }

        [DataMember(Name = "TN")]
        public string TagName { get; set; }
        [DataMember(Name = "TV")]
        public string TagValue { get; set; }
        [DataMember(Name = "TO", EmitDefaultValue = false)]
        public string TagNote { get; set; }
        [DataMember(Name = "UI")]
        public int UserId { get; set; }
        [DataMember(Name = "PR", EmitDefaultValue = false)]
        public decimal Price { get; set; }
        [DataMember(Name = "Q", EmitDefaultValue = false)]
        public decimal Quantity { get; set; }
        [DataMember(Name = "OI")]
        public int OrderTagGroupId { get; set; }
        [DataMember(Name = "MI", EmitDefaultValue = false)]
        public int MenuItemId { get; set; }
        [DataMember(Name = "AP", EmitDefaultValue = false)]
        public bool AddTagPriceToOrderPrice { get; set; }
        [DataMember(Name = "PN", EmitDefaultValue = false)]
        public string PortionName { get; set; }
        [DataMember(Name = "OK")]
        public string OrderKey { get; set; }
        [DataMember(Name = "FT", EmitDefaultValue = false)]
        public bool FreeTag { get; set; }       
        [DataMember(Name = "TF", EmitDefaultValue = false)]
        public bool TaxFree { get; set; }

        public void UpdatePrice(decimal orderTagPrice)
        {
            Price = orderTagPrice;
        }

        private string _shortName;
        public string ShortName
        {
            get { return _shortName ?? (_shortName = ToShort(TagValue)); }
        }

        private static OrderTagValue _empty;
        public static OrderTagValue Empty
        {
            get { return _empty ?? (_empty = new OrderTagValue { TagValue = "", OrderKey = "" }); }
        }

        private string ToShort(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            if (TagValue.Length < 3) return name;
            return name.Contains(" ") ? string.Join("", name.Split(' ').Select(x => char.IsNumber(x.ElementAt(0)) ? x : x.ElementAt(0).ToString())) : TagValue.Substring(0, 2);
        }
    }
}
