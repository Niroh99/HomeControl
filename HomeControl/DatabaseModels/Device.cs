using HomeControl.Integrations;
using HomeControl.Integrations.TPLink;
using HomeControl.Modeling;
using HomeControl.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HomeControl.Modeling;

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

        public IDevice Create()
        {
            switch (Type)
            {
                case DeviceType.TPLinkSmartPlug: return new SmartPlug(this, Hostname, Port);
                default: throw new NotImplementedException();
            }
        }
    }

    public enum DeviceType
    {
        TPLinkSmartPlug
    }
}