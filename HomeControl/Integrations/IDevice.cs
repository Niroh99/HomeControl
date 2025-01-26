using HomeControl.Models;

namespace HomeControl.Integrations
{
    public interface IDevice
    {
        Device Owner { get; }

        DeviceType DeviceType { get; }

        string DisplayName { get; }

        bool SupportsRename { get; }

        Task InitializeAsync();

        IEnumerable<Feature> GetExecutableFeatures();

        IEnumerable<IProperty> GetProperties();

        Task RenameAsync(string name);
    }
}