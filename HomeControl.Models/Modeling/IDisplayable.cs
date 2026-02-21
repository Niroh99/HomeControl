namespace HomeControl.Models.Modeling
{
    public interface IDisplayable
    {
        
    }

    public interface IDisplayable<T> : IDisplayable where T : IDisplay
    {

    }
}