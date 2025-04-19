using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.Database
{
    public abstract class IdentityKeyModel : DatabaseModel
    {
        [Key]
        [Column]
        public int Id { get => Get<int>(); set => Set(value); }
    }
}