namespace HomeControl.Events
{
    public abstract class EventHandler<T> where T : EventData
    {
        public abstract void HandleEvent(IServiceProvider serviceProvider, T data);
    }
}