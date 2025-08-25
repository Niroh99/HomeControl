using NTIH.Database.Modeling;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.Models.DatabaseModels
{
    [Table(nameof(DeviceOption))]
    public class DeviceOption : IdentityKeyModel
    {
        [Column]
        public int DeviceId { get => Get<int>(); set => Set(value); }

        [Column]
        public string Name { get => Get<string>(); set => Set(value); }
    }
}