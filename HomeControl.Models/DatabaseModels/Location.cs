using NTIH.Database.Modeling;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.Models.DatabaseModels
{
    [Table(nameof(Location))]
    public class Location : IdentityKeyModel
    {
        [Column]
        public string Name { get => Get<string>(); set => Set(value); }
    }
}