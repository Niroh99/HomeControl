using HomeControl.Database;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(Location))]
    public class Location : IdentityKeyModel
    {
        [Column]
        public string Name { get => Get<string>(); set => Set(value); }
    }
}