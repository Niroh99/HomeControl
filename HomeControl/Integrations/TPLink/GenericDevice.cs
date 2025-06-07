using HomeControl.Integrations.TPLink.JSON;

namespace HomeControl.Integrations.TPLink
{
    public abstract class GenericDevice<T>(string hostname, int port = 9999) : Device(hostname, port) where T : SysInfo
    {
        protected T SysInfo { get => (T)_sysInfo; }

        protected override Type SysInfoType => typeof(T);
    }
}