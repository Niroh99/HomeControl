using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTIH.Database.Modeling
{
    public abstract class StringKeyModel : DatabaseModel
    {
        [Key]
        [Column]
        public string Id { get => Get<string>(); set => Set(value); }
    }
}