using HomeControl.Integrations.TPLink.JSON;

namespace HomeControl.Integrations.TPLink
{
    public abstract class GenericDevice<T> : Device where T : SysInfo
    {
        public GenericDevice(string hostname, int port = 9999) : base(hostname, port)
        {
        }

        protected T SysInfo { get => (T)_sysInfo; }

        protected override Type SysInfoType => typeof(T);
    }
}