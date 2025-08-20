namespace HomeControl.Modeling
{
    public interface IDisplayable
    {
        string Display { get; }

        string AdditionalInfo { get; }

        Task CreateDisplay(IServiceProvider serviceProvider);
    }
}
