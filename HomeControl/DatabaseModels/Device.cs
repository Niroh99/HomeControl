using HomeControl.Database;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(Device))]
    public class Device : IdentityKeyModel
    {
        [Column]
        public DeviceType Type { get => Get<DeviceType>(); set => Set(value); }

        [Column]
        public string Hostname { get => Get<string>(); set => Set(value); }

        [Column]
        public int Port { get => Get<int>(); set => Set(value); }
    }

    public enum DeviceType
    {
        TPLinkSmartPlug
    }
}