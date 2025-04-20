using HomeControl.Modeling;

namespace HomeControl.Database
{
    public abstract class DatabaseModel : Model
    {
        protected IDatabaseConnection DB { get; private set; }

        public void Track(IDatabaseConnection db) => DB = db;

        public virtual void OnInserting()
        {

        }

        public virtual void OnUpdating()
        {

        }

        public virtual void OnDeleting()
        {

        }
    }
}