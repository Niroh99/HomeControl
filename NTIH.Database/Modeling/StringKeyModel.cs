using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTIH.Database.Modeling
{
    public abstract class StringKeyModel : DatabaseTableModel
    {
        [Key]
        [Column]
        public string Id { get => Get<string>(); set => Set(value); }
    }
}