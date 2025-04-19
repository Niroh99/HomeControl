using HomeControl.DatabaseModels;

namespace HomeControl.Integrations
{
    public interface IIntegrationDevice
    {
        Device Owner { get; }

        DeviceType DeviceType { get; }

        bool AllowCaching { get; }

        string DisplayName { get; }

        bool SupportsRename { get; }

        DeviceInitilizationState InitilizationState { get; }

        string InitializationError { get; }

        Task InitializeAsync();

        IEnumerable<Feature> GetFeatures();

        IEnumerable<Feature> GetExecutableFeatures();

        IEnumerable<IProperty> GetProperties();

        Task ExecuteFeatureAsync(string featureName);

        Task RenameAsync(string name);
    }
}