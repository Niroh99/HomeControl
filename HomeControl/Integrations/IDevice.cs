using HomeControl.Models;

namespace HomeControl.Integrations
{
    public interface IDevice
    {
        Device Owner { get; }

        DeviceType DeviceType { get; }

        string DisplayName { get; }

        bool SupportsRename { get; }

        IEnumerable<Feature> GetExecutableFeatures();

        IEnumerable<IProperty> GetProperties();

        void Rename(string name);
    }
}