using HomeControl.DatabaseModels;
using HomeControl.Integrations.TPLink.JSON;

namespace HomeControl.Integrations.TPLink
{
    public abstract class Device(string hostname, int port = 9999) : IIntegrationDevice
    {
        public const string ProtocolMessageSystem = "system";

        public const string GetSysInfoCommand = "get_sysinfo";

        public const string SetDeviceAliasCommand = "set_dev_alias";
        public const string SetDeviceAliasArgument = "alias";

        public abstract DatabaseModels.Device Owner { get; }

        public abstract DeviceType DeviceType { get; }

        public string Hostname { get; } = hostname;

        public int Port { get; } = port;

        protected SysInfo _sysInfo;

        public string Alias { get => _sysInfo.Alias; }

        public string SoftwareVersion { get => _sysInfo.SoftwareVersion; }

        public string HardwareVersion { get => _sysInfo.HardwareVersion; }

        public string Type { get => _sysInfo.Type; }

        public string Model { get => _sysInfo.Model; }

        public string MacAddress { get => _sysInfo.MacAddress; }

        public string DeviceName { get => _sysInfo.DeviceName; }

        public string HardwareId { get => _sysInfo.HardwareId; }

        public string FirmwareId { get => _sysInfo.FirmwareId; }

        public string DeviceId { get => _sysInfo.DeviceId; }

        public string OemId { get => _sysInfo.OemId; }

        public int RSSI { get => _sysInfo.Rssi; }

        public double Latitude { get => _sysInfo.Latitude; }

        public double Longitude { get => _sysInfo.Longitude; }

        protected abstract Type SysInfoType { get; }

        public string DisplayName => Alias;

        public bool SupportsRename { get => true; }

        public async Task InitializeAsync()
        {
            var message = new ProtocolMessage(ProtocolMessageSystem, GetSysInfoCommand, null, null);

            _sysInfo = (SysInfo)await message.ExecuteAsync(Hostname, Port, SysInfoType);
        }

        public virtual IEnumerable<IProperty> GetProperties()
        {
            yield return new SingleProperty("Hostname:", Hostname);
            yield return new SingleProperty("Port:", Port.ToString());
            yield return new SingleProperty("Software-Version:", SoftwareVersion);
            yield return new SingleProperty("Hardware-Version:", HardwareVersion);
            yield return new SingleProperty("Type:", Type);
            yield return new SingleProperty("Model:", Model);
            yield return new SingleProperty("MAC Address:", MacAddress);
            yield return new SingleProperty("Device Name:", DeviceName);
            yield return new SingleProperty("Hardware Id:", HardwareId);
            yield return new SingleProperty("Firmware Id:", FirmwareId);
            yield return new SingleProperty("Device Id", DeviceId);
            yield return new SingleProperty("OEM Id:", OemId);
            yield return new SingleProperty("RSSI:", RSSI.ToString());
            yield return new MultiProperty("Location:",
            [
                new SingleProperty("Latitude", Latitude.ToString()),
                new SingleProperty("Longitude", Longitude.ToString())
            ]);
        }

        public abstract IEnumerable<Feature> GetExecutableFeatures();

        public abstract Task ExecuteFeatureAsync(string featureName);

        public async Task RenameAsync(string name)
        {
            var message = new ProtocolMessage(ProtocolMessageSystem, SetDeviceAliasCommand, SetDeviceAliasArgument, name);

            await message.ExecuteAsync(Hostname, Port);
        }
    }
}