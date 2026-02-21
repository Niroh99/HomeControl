namespace HomeControl.Models.Modeling
{
    public interface IDisplay
    {
        string Display { get; }

        string AdditionalInfo { get; }

        Task Create(object displayable, IServiceProvider serviceProvider);
    }

    public interface IDisplay<T> : IDisplay where T : IDisplayable
    {

    }
}