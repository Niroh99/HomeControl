using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
    public class DeviceOption : IdentityKeyModel
    {
        [Column]
        public int DeviceId { get => Get<int>(); set => Set(value); }

        [Column]
        public string Name { get => Get<string>(); set => Set(value); }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(DeviceId))]
        public Device Device { get => Get<Device>(); }
    }
}