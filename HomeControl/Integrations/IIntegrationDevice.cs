using HomeControl.DatabaseModels;

namespace HomeControl.Integrations
{
    public interface IIntegrationDevice
    {
        Device Owner { get; }

        DeviceType DeviceType { get; }

        string DisplayName { get; }

        bool SupportsRename { get; }

        Task InitializeAsync();

        IEnumerable<Feature> GetExecutableFeatures();

        IEnumerable<IProperty> GetProperties();

        Task ExecuteFeatureAsync(string featureName);

        Task RenameAsync(string name);
    }
}