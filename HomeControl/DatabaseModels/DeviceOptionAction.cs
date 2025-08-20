using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(DeviceOptionAction))]
    public class DeviceOptionAction : Action
    {
        [Column]
        public int DeviceOptionId { get => Get<int>(); set => Set(value); }
    }
}