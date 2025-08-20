using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTIH.Database.Modeling
{
    public abstract class IdentityKeyModel : DatabaseModel
    {
        [Key]
        [Column]
        public int Id { get => Get<int>(); set => Set(value); }
    }
}