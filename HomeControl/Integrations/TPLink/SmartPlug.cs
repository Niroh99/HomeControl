using HomeControl.DatabaseModels;
using HomeControl.Integrations.TPLink.JSON;
using System.Text.Json.Serialization;

namespace HomeControl.Integrations.TPLink
{
    public sealed class SmartPlug : GenericDevice<SmartPlug.SmartPlugSysInfo>
    {
        private const string SetRelayStateCommand = "set_relay_state";
        private const string SetRelayStateCommandArgument = "state";

        public const string TurnOnFeatureName = "Turn On";
        public const string TurnOffFeatureName = "Turn Off";

        public class SmartPlugSysInfo : SysInfo
        {
            [JsonPropertyName("on_time")]
            public int? OnTime { get; set; }

            [JsonPropertyName("relay_state")]
            public int RelayState { get; set; }
        }

        public SmartPlug(string hostname, int port = 9999) : this(null, hostname, port)
        {
            
        }

        public SmartPlug(DatabaseModels.Device owner, string hostname, int port = 9999) : base(hostname, port)
        {
            _owner = owner;
            _turnOn = new Feature(TurnOnFeatureName, SetPoweredOn);
            _turnOff = new Feature(TurnOffFeatureName, SetPoweredOff);
        }

        private Feature _turnOn;
        private Feature _turnOff;

        private DatabaseModels.Device _owner;
        public override DatabaseModels.Device Owner => _owner;

        public override DeviceType DeviceType => DeviceType.TPLinkSmartPlug;

        public bool OutletPowered { get => SysInfo.RelayState == 1; }

        public int? TurnedOnSince { get => SysInfo.OnTime; }

        public async Task SetPoweredOn()
        {
            var message = new ProtocolMessage(ProtocolMessageSystem, SetRelayStateCommand, SetRelayStateCommandArgument, 1);

            await message.ExecuteAsync(Hostname, Port);
        }

        public async Task SetPoweredOff()
        {
            var message = new ProtocolMessage(ProtocolMessageSystem, SetRelayStateCommand, SetRelayStateCommandArgument, 0);

            await message.ExecuteAsync(Hostname, Port);
        }

        public override IEnumerable<Feature> GetExecutableFeatures()
        {
            if (_sysInfo == null) yield break;

            if (OutletPowered) yield return _turnOff;
            else yield return _turnOn;
        }

        public override IEnumerable<IProperty> GetProperties()
        {
            if (OutletPowered) yield return new SingleProperty("Turned on since:", $"{TurnedOnSince} s");

            foreach (var baseProperty in base.GetProperties())
            {
                yield return baseProperty;
            }
        }

        public override async Task ExecuteFeatureAsync(string featureName)
        {
            switch (featureName)
            {
                case TurnOnFeatureName: await _turnOn.Execute(); break;
                case TurnOffFeatureName: await _turnOff.Execute(); break;
            }
        }
    }
}