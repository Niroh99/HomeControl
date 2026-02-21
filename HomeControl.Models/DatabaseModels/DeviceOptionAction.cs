using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
    public class DeviceOptionAction : Action
    {
        [Column]
        public int DeviceOptionId { get => Get<int>(); set => Set(value); }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(DeviceOptionId))]
        public DeviceOption DeviceOption { get => Get<DeviceOption>(); }
    }
}