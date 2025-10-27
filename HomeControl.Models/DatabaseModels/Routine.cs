using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
    public class Routine : IdentityKeyModel
    {
        [Column]
        public string Name { get => Get<string>(); set => Set(value); }

        [Column]
        public bool IsActive { get => Get<bool>(); set => Set(value); }

        [Column]
        public DateTime? LastExecution { get => Get<DateTime?>(); set => Set(value); }
    }
}