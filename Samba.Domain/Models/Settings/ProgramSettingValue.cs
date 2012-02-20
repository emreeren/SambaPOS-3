using System.ComponentModel.DataAnnotations;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class ProgramSettingValue : Entity
    {
        [StringLength(250)]
        public string Value { get; set; }
    }
}
