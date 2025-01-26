using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.Models
{
    public class StringKeyModel : Model
    {
        [Key]
        [Column()]
        public string Id { get => Get<string>(); set => Set(value); }
    }
}