using System.Text.Json.Serialization;

namespace NTIH.Database.Modeling
{
    public abstract class DatabaseTableModel : DatabaseModel
    {
        [JsonIgnore]
        public bool IsTracked { get => DB != null; }

        [JsonIgnore]
        public DatabaseConnection DB { get; private set; }

        internal void Track(DatabaseConnection db) => DB = db;

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