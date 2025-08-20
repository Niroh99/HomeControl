namespace NTIH.Database.Modeling
{
    public abstract class DatabaseModel : NTIH.Modeling.Model
    {
        public bool IsTracked { get => DB != null; }

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