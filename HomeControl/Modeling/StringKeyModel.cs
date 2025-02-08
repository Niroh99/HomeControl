using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.Modeling
{
    public abstract class StringKeyModel : Model
    {
        [Key]
        [Column()]
        public string Id { get => Get<string>(); set => Set(value); }
    }
}