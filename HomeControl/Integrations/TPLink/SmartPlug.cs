using HomeControl.Integrations.TPLink.JSON;
using HomeControl.Models;
using System.Text.Json.Serialization;

namespace HomeControl.Integrations.TPLink
{
    public sealed class SmartPlug : GenericDevice<SmartPlug.SmartPlugSysInfo>
    {
        private const string SetRelayStateCommand = "set_relay_state";
        private const string SetRelayStateCommandArgument = "state";

        public class SmartPlugSysInfo : SysInfo
        {
            [JsonPropertyName("on_time")]
            public int? OnTime { get; set; }

            [JsonPropertyName("relay_state")]
            public int RelayState { get; set; }
        }

        public SmartPlug(string hostname, int port = 9999) : base(hostname, port)
        {

        }

        public SmartPlug(Models.Device owner, string hostname, int port = 9999) : base(hostname, port)
        {
            _owner = owner;
        }

        private Models.Device _owner;
        public override Models.Device Owner => _owner;

        public override DeviceType DeviceType => DeviceType.TPLinkSmartPlug;

        public bool OutletPowered { get => SysInfo.RelayState == 1; }

        public int? TurnedOnSince { get => SysInfo.OnTime; }

        public void SetPoweredOn()
        {
            var message = new ProtocolMessage(ProtocolMessageSystem, SetRelayStateCommand, SetRelayStateCommandArgument, 1);

            message.Execute(Hostname, Port);
        }

        public void SetPoweredOff()
        {
            var message = new ProtocolMessage(ProtocolMessageSystem, SetRelayStateCommand, SetRelayStateCommandArgument, 0);

            message.Execute(Hostname, Port);
        }

        public override IEnumerable<Feature> GetExecutableFeatures()
        {
            if (OutletPowered) yield return new Feature("Turn Off", SetPoweredOff);
            else yield return new Feature("Turn On", SetPoweredOn);
        }

        public override IEnumerable<IProperty> GetProperties()
        {
            if (OutletPowered) yield return new SingleProperty("Turned on since:", $"{TurnedOnSince} s");

            foreach (var baseProperty in GetBaseProperties())
            {
                yield return baseProperty;
            }
        }
    }
}