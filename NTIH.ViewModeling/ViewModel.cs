using NTIH.Modeling;

namespace NTIH.ViewModeling
{
    public abstract class ViewModel : Model
    {
        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }
    }
}