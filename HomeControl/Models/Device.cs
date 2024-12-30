using HomeControl.Integrations;
using HomeControl.Integrations.TPLink;
using HomeControl.Modeling;
using HomeControl.Sql;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.Models
{
    [Table(nameof(Device))]
    public class Device : KeyedGenericSqLiteModel<Device, int>
    {
        [Key]
        [Identity]
        [Column(Database.RowIdColumnName)]
        public int Id { get => Get<int>(); set => Set(value); }

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