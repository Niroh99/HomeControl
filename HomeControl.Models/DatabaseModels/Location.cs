using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
    public class Location : IdentityKeyModel
    {
        [Column]
        public string Name { get => Get<string>(); set => Set(value); }
    }
}